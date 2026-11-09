<#
.SYNOPSIS
  End-to-end T-SQL perf run for docs/SQL-PERF.md on a THROWAWAY SQL Server 2022 container.

.DESCRIPTION
  1. docker run clearpay-jobprep-sql (port 14333, no volume)        - skipped if it already runs
  2. CREATE DATABASE ClearPayPerf; EF migrations up to the pre-change schema (AddLinkedInstrumentScheme)
  3. 01-seed.sql  (600,000 LedgerEntry rows, 5,001 wallets)
  4. Install the procs from src/ClearPay.Infrastructure/Persistence/Sql/*.sql
  5. measure.ps1 -State no-index  (only the clustered PK on LedgerEntry.Id)
  6. measure.ps1 -State before    (pre-change schema: narrow IX_LedgerEntry_WalletId_CreatedAt)
  7. dotnet ef database update     (AddLedgerReadProcedures: covering / pair / day indexes + procs)
  8. measure.ps1 -State after
  9. docker rm -f the container (unless -KeepContainer)

  Never point this at D:/ClearPay data or any real database.
#>
param(
    [switch]$KeepContainer,
    [string]$Container = 'clearpay-jobprep-sql',
    [int]$Port = 14333,
    [string]$Password = $(if ($env:CLEARPAY_PERF_SA_PASSWORD) { $env:CLEARPAY_PERF_SA_PASSWORD } else { 'JobPrep_Dev1!' }),
    [string]$Sqlcmd = 'C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE'
)

$ErrorActionPreference = 'Stop'
$repo = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$server = "localhost,$Port"
$db = 'ClearPayPerf'
$efConn = "Server=$server;Database=$db;User Id=sa;Password=$Password;TrustServerCertificate=True"

function Sql([string]$query, [string]$database = 'master') {
    & $Sqlcmd -S $server -U sa -P $Password -C -d $database -b -Q $query
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed: $query" }
}

function SqlFile([string]$path, [string]$database) {
    & $Sqlcmd -S $server -U sa -P $Password -C -d $database -b -i $path
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed: $path" }
}

function Ef([string[]]$efArgs) {
    & dotnet ef database update @efArgs --context ClearPayDbContext `
        --project (Join-Path $repo 'src\ClearPay.Infrastructure') --connection $efConn
    if ($LASTEXITCODE -ne 0) { throw 'dotnet ef database update failed' }
}

$running = docker ps --filter "name=^$Container$" --format '{{.Names}}'
if (-not $running) {
    docker run -d --name $Container -e ACCEPT_EULA=Y -e "MSSQL_SA_PASSWORD=$Password" `
        -p "${Port}:1433" mcr.microsoft.com/mssql/server:2022-latest | Out-Null
}

$ready = $false
for ($i = 0; $i -lt 60 -and -not $ready; $i++) {
    # Windows PowerShell turns native stderr into an error record; ignore it while SQL Server boots.
    $ErrorActionPreference = 'Continue'
    & $Sqlcmd -S $server -U sa -P $Password -C -l 2 -Q 'SELECT 1' 2>&1 | Out-Null
    $code = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    if ($code -eq 0) { $ready = $true } else { Start-Sleep -Seconds 2 }
}
if (-not $ready) { throw 'SQL Server container did not become ready.' }

try {
    Sql "IF DB_ID('$db') IS NOT NULL BEGIN ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db]; END; CREATE DATABASE [$db];"
    Ef @('AddLinkedInstrumentScheme')
    SqlFile (Join-Path $PSScriptRoot '01-seed.sql') $db
    Get-ChildItem (Join-Path $repo 'src\ClearPay.Infrastructure\Persistence\Sql\*.sql') | ForEach-Object { SqlFile $_.FullName $db }

    Sql 'DROP INDEX IX_LedgerEntry_WalletId_CreatedAt ON dbo.LedgerEntry;' $db
    & (Join-Path $PSScriptRoot 'measure.ps1') -State 'no-index' -Server $server -Password $Password

    Sql 'CREATE INDEX IX_LedgerEntry_WalletId_CreatedAt ON dbo.LedgerEntry (WalletId, CreatedAt);' $db
    & (Join-Path $PSScriptRoot 'measure.ps1') -State 'before' -Server $server -Password $Password

    Ef @()
    Sql "SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.LedgerEntry') ORDER BY name; SELECT name FROM sys.procedures ORDER BY name;" $db
    & (Join-Path $PSScriptRoot 'measure.ps1') -State 'after' -Server $server -Password $Password
}
finally {
    if (-not $KeepContainer) {
        docker rm -f $Container | Out-Null
        Write-Host "Removed container $Container"
    }
}
