<#
.SYNOPSIS
  Runs the ClearPay ledger read queries against a THROWAWAY SQL Server and records
  STATISTICS IO / TIME and the actual execution plan (STATISTICS XML) for each one.

.DESCRIPTION
  Used by scripts/perf/run-perf.ps1 for docs/SQL-PERF.md. Windows PowerShell 5.1 compatible
  (System.Data.SqlClient, no extra modules).
  - Logical reads: sum of "Table '...'. Scan count n, logical reads n" messages (one run, warm cache).
  - CPU / elapsed: largest "SQL Server Execution Times" message of the batch (the outer EXEC /
    sp_executesql, which includes its statements), median of -Runs runs, warm cache, STATISTICS XML OFF.
  - Client ms: wall clock in this script incl. sending the rows over TCP (median of -Runs).
  - Plan cache is NOT cleared between queries: the q1/q2 procs are compiled for the first wallet
    that calls them (customer), then reused for the treasury - like production. See docs/SQL-PERF.md.
  - Plan: one extra run with STATISTICS XML ON; all statements merged into one .sqlplan file
    (open in SSMS / Azure Data Studio).

.EXAMPLE
  ./measure.ps1 -State after -PlanDir ../../docs/sqlplans -ResultDir ./results
#>
param(
    [Parameter(Mandatory = $true)][string]$State,
    [string]$Server = 'localhost,14333',
    [string]$Database = 'ClearPayPerf',
    [string]$User = 'sa',
    # Throwaway container password only (dev default). Never a real server.
    [string]$Password = $(if ($env:CLEARPAY_PERF_SA_PASSWORD) { $env:CLEARPAY_PERF_SA_PASSWORD } else { 'JobPrep_Dev1!' }),
    [int]$Runs = 7,
    [string]$PlanDir = (Join-Path $PSScriptRoot '..\..\docs\sqlplans'),
    [string]$ResultDir = (Join-Path $PSScriptRoot 'results')
)

$ErrorActionPreference = 'Stop'
$showplanNs = 'http://schemas.microsoft.com/sqlserver/2004/07/showplan'

New-Item -ItemType Directory -Force -Path $PlanDir | Out-Null
New-Item -ItemType Directory -Force -Path $ResultDir | Out-Null

$cs = "Server=$Server;Database=$Database;User Id=$User;Password=$Password;TrustServerCertificate=True;Encrypt=True"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$script:messages = New-Object System.Collections.Generic.List[string]
$conn.add_InfoMessage({ param($sender, $e) foreach ($m in $e.Errors) { $script:messages.Add($m.Message) } })
$conn.Open()
# Throwaway server: start each state with an empty plan cache so the compile order is the same every run.
$freeCache = $conn.CreateCommand(); $freeCache.CommandText = "DBCC FREEPROCCACHE WITH NO_INFOMSGS;"; [void]$freeCache.ExecuteNonQuery()

function Invoke-Scalar([string]$sql) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    return $cmd.ExecuteScalar()
}

function Invoke-Batch([string]$sql, [bool]$withPlan) {
    $script:messages.Clear()
    $plans = New-Object System.Collections.Generic.List[string]
    $set = 'SET STATISTICS IO ON; SET STATISTICS TIME ON;'
    if ($withPlan) { $set += ' SET STATISTICS XML ON;' }
    $cmd = $conn.CreateCommand()
    $cmd.CommandTimeout = 300
    $cmd.CommandText = "$set`n$sql"
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $reader = $cmd.ExecuteReader()
    $rows = 0
    do {
        $isPlan = $reader.FieldCount -eq 1 -and $reader.GetName(0) -like '*XML Showplan*'
        while ($reader.Read()) {
            if ($isPlan) { $plans.Add($reader.GetString(0)) } else { $rows++ }
        }
    } while ($reader.NextResult())
    $reader.Close()
    $sw.Stop()

    $captured = $script:messages.ToArray()
    $off = $conn.CreateCommand()
    $off.CommandText = 'SET STATISTICS IO OFF; SET STATISTICS TIME OFF; SET STATISTICS XML OFF;'
    [void]$off.ExecuteNonQuery()

    $reads = @{}
    $times = @()
    foreach ($m in $captured) {
        foreach ($match in [regex]::Matches($m, "Table '([^']+)'\. Scan count (\d+), logical reads (\d+)")) {
            $t = $match.Groups[1].Value
            if (-not $reads.ContainsKey($t)) { $reads[$t] = 0 }
            $reads[$t] += [int]$match.Groups[3].Value
        }
        $tm = [regex]::Match($m, 'SQL Server Execution Times:\s*CPU time = (\d+) ms,\s*elapsed time = (\d+) ms')
        if ($tm.Success) { $times += , @([int]$tm.Groups[1].Value, [int]$tm.Groups[2].Value) }
    }
    # The outer EXEC / sp_executesql time includes its inner statements, so it is the largest one.
    $cpuMax = ($times | ForEach-Object { $_[0] } | Measure-Object -Maximum).Maximum
    $elapsedMax = ($times | ForEach-Object { $_[1] } | Measure-Object -Maximum).Maximum
    return [pscustomobject]@{
        Rows     = $rows
        Reads    = $reads
        CpuMs    = [int]$cpuMax
        ElapsedMs = [int]$elapsedMax
        ClientMs = $sw.Elapsed.TotalMilliseconds
        Plans    = $plans
    }
}

function Merge-Plans($plans) {
    $doc = New-Object System.Xml.XmlDocument
    $doc.LoadXml($plans[0])
    $nsm = New-Object System.Xml.XmlNamespaceManager $doc.NameTable
    $nsm.AddNamespace('p', $showplanNs)
    $target = $doc.SelectSingleNode('//p:BatchSequence/p:Batch/p:Statements', $nsm)
    for ($i = 1; $i -lt $plans.Count; $i++) {
        $other = New-Object System.Xml.XmlDocument
        $other.LoadXml($plans[$i])
        $onsm = New-Object System.Xml.XmlNamespaceManager $other.NameTable
        $onsm.AddNamespace('p', $showplanNs)
        foreach ($stmt in $other.SelectNodes('//p:BatchSequence/p:Batch/p:Statements/*', $onsm)) {
            [void]$target.AppendChild($doc.ImportNode($stmt, $true))
        }
    }
    return $doc
}

function Get-AccessOps([System.Xml.XmlDocument]$doc) {
    $nsm = New-Object System.Xml.XmlNamespaceManager $doc.NameTable
    $nsm.AddNamespace('p', $showplanNs)
    $ops = New-Object System.Collections.Generic.List[string]
    foreach ($relOp in $doc.SelectNodes('//p:RelOp', $nsm)) {
        $scan = $relOp.SelectSingleNode('p:IndexScan | p:TableScan', $nsm)
        if ($null -eq $scan) { continue }
        $obj = $scan.SelectSingleNode('p:Object', $nsm)
        if ($null -eq $obj) { continue }
        $table = $obj.GetAttribute('Table').Trim('[', ']')
        $index = $obj.GetAttribute('Index').Trim('[', ']')
        if ($table -notin @('LedgerEntry', 'Wallet')) { continue }
        $op = $relOp.GetAttribute('PhysicalOp')
        if ($scan.GetAttribute('Lookup') -eq '1') { $op = 'Key Lookup' }
        $actual = 0
        foreach ($t in $relOp.SelectNodes('p:RunTimeInformation/p:RunTimeCountersPerThread', $nsm)) {
            $actual += [long]$t.GetAttribute('ActualRows')
        }
        $ops.Add("$op $table.$index (rows $actual)")
    }
    return $ops
}

# --- parameters taken from the seeded data -------------------------------------------------
$treasury = Invoke-Scalar "SELECT CONVERT(nvarchar(36), Id) FROM dbo.Wallet WHERE UserId = N'perf-treasury'"
$customer = Invoke-Scalar "SELECT CONVERT(nvarchar(36), Id) FROM dbo.Wallet WHERE UserId = N'perf-user-00042'"
if (-not $treasury -or -not $customer) { throw 'Seed missing: run 01-seed.sql first.' }
$month = "'2026-10-01T00:00:00+00:00'"

$queries = [ordered]@{
    'q0-legacy-ef-load-treasury' = @"
-- Old SqlWalletReader / SqlActivityReader path: EF loads every ledger row of the wallet, C# sums/pages.
-- Same shape EF Core sends (sp_executesql with a typed parameter).
EXEC sp_executesql
    N'SELECT [l].[Id], [l].[Amount], [l].[CorrelationId], [l].[CreatedAt], [l].[Description], [l].[Kind], [l].[PairId], [l].[TransferId], [l].[WalletId] FROM [LedgerEntry] AS [l] WHERE [l].[WalletId] = @__wallet_Id_0',
    N'@__wallet_Id_0 uniqueidentifier',
    @__wallet_Id_0 = '$treasury';
"@
    'q1-summary-customer'        = "EXEC dbo.usp_Wallet_GetSummary @WalletId = '$customer', @MonthStart = $month;"
    'q1-summary-treasury'        = "EXEC dbo.usp_Wallet_GetSummary @WalletId = '$treasury', @MonthStart = $month;"
    'q2-statement-customer-p1'   = "EXEC dbo.usp_Wallet_GetStatementPage @WalletId = '$customer', @Offset = 0, @PageSize = 20;"
    'q2-statement-treasury-p1'   = "EXEC dbo.usp_Wallet_GetStatementPage @WalletId = '$treasury', @Offset = 0, @PageSize = 20;"
    'q2-statement-treasury-filtered-p3' = "EXEC dbo.usp_Wallet_GetStatementPage @WalletId = '$treasury', @From = '2026-08-01T00:00:00+00:00', @To = '2026-09-01T00:00:00+00:00', @Kind = 1, @Offset = 40, @PageSize = 20;"
    'q3-daily-totals'            = "EXEC dbo.usp_Ledger_GetDailyTotals @DayStart = '2026-10-15T00:00:00+00:00', @DayEnd = '2026-10-16T00:00:00+00:00';"
}

$results = New-Object System.Collections.Generic.List[object]
foreach ($name in $queries.Keys) {
    $sql = $queries[$name]
    [void](Invoke-Batch $sql $false)                       # warm-up (data pages in buffer pool)
    $timed = @()
    for ($i = 0; $i -lt $Runs; $i++) { $timed += Invoke-Batch $sql $false }
    $cpu = ($timed | ForEach-Object CpuMs | Sort-Object)[[int][math]::Floor($Runs / 2)]
    $elapsed = ($timed | ForEach-Object ElapsedMs | Sort-Object)[[int][math]::Floor($Runs / 2)]
    $client = ($timed | ForEach-Object ClientMs | Sort-Object)[[int][math]::Floor($Runs / 2)]

    $planRun = Invoke-Batch $sql $true
    $doc = Merge-Plans $planRun.Plans
    $planPath = Join-Path $PlanDir "$State-$name.sqlplan"
    $doc.Save($planPath)

    $readsTotal = 0
    $readsText = @()
    foreach ($k in ($timed[0].Reads.Keys | Sort-Object)) {
        $readsTotal += $timed[0].Reads[$k]
        $readsText += "$k=$($timed[0].Reads[$k])"
    }

    $results.Add([pscustomobject]@{
        State        = $State
        Query        = $name
        RowsReturned = $timed[0].Rows
        LogicalReads = $readsTotal
        ReadsByTable = ($readsText -join ', ')
        CpuMsMedian  = $cpu
        ElapsedMsMedian = $elapsed
        ClientMsMedian  = [math]::Round($client, 1)
        PlanAccess   = ((Get-AccessOps $doc) -join ' | ')
        PlanFile     = "docs/sqlplans/$State-$name.sqlplan"
    })
    Write-Host ("{0,-36} reads={1,7} cpu={2,4}ms elapsed={3,4}ms client={4,7}ms" -f $name, $readsTotal, $cpu, $elapsed, [math]::Round($client, 1))
}

$conn.Close()
$out = Join-Path $ResultDir "$State.json"
$results | ConvertTo-Json -Depth 4 | Set-Content -Encoding utf8 $out
Write-Host "Saved $out"
