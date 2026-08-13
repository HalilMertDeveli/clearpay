# Infra connectivity smoke for local SQL Server / MySQL / Oracle.
# Not part of `dotnet test` (CI has no engines). Re-run:
#   powershell -ExecutionPolicy Bypass -File scripts/db-smoke.ps1
$ErrorActionPreference = "Continue"
$fail = 0
function Pass($n, $d) { Write-Host "PASS  $n  $d" -ForegroundColor Green }
function Fail($n, $d) { Write-Host "FAIL  $n  $d" -ForegroundColor Red; $script:fail++ }

Write-Host "=== ClearPay local DB smoke ==="

# SQL Server: Windows native (Integrated) then TCP 1433
$sqlOk = $false
$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
if ($sqlcmd) {
  $out = & sqlcmd -S "localhost" -E -Q "SELECT @@SERVERNAME" -W -h -1 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0 -and $out -notmatch "Sqlcmd: Error") {
    Pass "sqlserver" "localhost (Windows auth) $($out.Trim().Split("`n")[0])"
    $sqlOk = $true
  } else {
    $sa = $env:MSSQL_SA_PASSWORD
    if (-not $sa) { $sa = "ClearPay_Dev1!" }
    $out2 = & sqlcmd -S "localhost,1433" -U sa -P $sa -Q "SELECT @@SERVERNAME" -W -h -1 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -and $out2 -notmatch "Sqlcmd: Error") {
      Pass "sqlserver" "localhost,1433 (sa) $($out2.Trim().Split("`n")[0])"
      $sqlOk = $true
    } else {
      Fail "sqlserver" "sqlcmd localhost and localhost,1433 failed. $out $out2"
    }
  }
} else {
  $tn = Test-NetConnection -ComputerName 127.0.0.1 -Port 1433 -WarningAction SilentlyContinue
  if ($tn.TcpTestSucceeded) { Pass "sqlserver" "TCP 1433 open (no sqlcmd)" ; $sqlOk = $true }
  else { Fail "sqlserver" "sqlcmd missing and port 1433 closed" }
}

# MySQL 3306
$mysqlBin = "C:\Program Files\MySQL\MySQL Server 8.4\bin"
if (Test-Path $mysqlBin) { $env:Path = "$mysqlBin;" + $env:Path }
$mysql = Get-Command mysql -ErrorAction SilentlyContinue
$mysqladmin = Get-Command mysqladmin -ErrorAction SilentlyContinue
$mp = $env:MYSQL_ROOT_PASSWORD
if (-not $mp) { $mp = "ClearPay_Dev1!" }
$mysqlOk = $false
if ($mysqladmin) {
  $o = & mysqladmin --protocol=TCP -h 127.0.0.1 -P 3306 -uroot "-p$mp" ping 2>&1 | Out-String
  if ($o -match "mysqld is alive") { Pass "mysql" "3306 mysqladmin ping"; $mysqlOk = $true }
  else { Fail "mysql" $o.Trim() }
} elseif ($mysql) {
  $o = & mysql --protocol=TCP -h 127.0.0.1 -P 3306 -uroot "-p$mp" -e "SELECT 1 AS ok;" 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0) { Pass "mysql" "3306 SELECT 1"; $mysqlOk = $true }
  else { Fail "mysql" $o.Trim() }
} else {
  $tn = Test-NetConnection -ComputerName 127.0.0.1 -Port 3306 -WarningAction SilentlyContinue
  if ($tn.TcpTestSucceeded) { Pass "mysql" "TCP 3306 open (no mysql client)"; $mysqlOk = $true }
  else { Fail "mysql" "port 3306 closed (Docker: docker compose -f docker-compose.databases.yml up -d)" }
}

# Oracle 1521
$oraOk = $false
$docker = $null
foreach ($c in @(
  "C:\Program Files\Docker\Docker\resources\bin\docker.exe",
  (Get-Command docker -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source)
)) { if ($c -and (Test-Path $c)) { $docker = $c; break } }

if ($docker) {
  $info = & $docker exec clearpay-oracle healthcheck.sh 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0) { Pass "oracle" "container healthcheck.sh"; $oraOk = $true }
  else {
    $tn = Test-NetConnection -ComputerName 127.0.0.1 -Port 1521 -WarningAction SilentlyContinue
    if ($tn.TcpTestSucceeded) { Pass "oracle" "TCP 1521 open"; $oraOk = $true }
    else { Fail "oracle" "1521 closed / container not healthy. Docker WSL reboot may be required. $info" }
  }
} else {
  $tn = Test-NetConnection -ComputerName 127.0.0.1 -Port 1521 -WarningAction SilentlyContinue
  if ($tn.TcpTestSucceeded) { Pass "oracle" "TCP 1521 open"; $oraOk = $true }
  else { Fail "oracle" "port 1521 closed -- Oracle image needs Docker Desktop (WSL2 reboot)" }
}

Write-Host "=== summary: fail=$fail sql=$sqlOk mysql=$mysqlOk oracle=$oraOk ==="
if ($fail -gt 0) { exit 1 } else { exit 0 }