# After Windows reboot (VMP/WSL pending). Starts Compose SQL + Redis + Rabbit.
# MySQL/Oracle: docker compose -f docker-compose.databases.yml up -d
# Native MySQL84 already on :3306 — do not start Compose mysql until that service is stopped.
$ErrorActionPreference = "Stop"
$docker = "C:\Program Files\Docker\Docker\resources\bin\docker.exe"
$desktop = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
$root = Split-Path $PSScriptRoot -Parent

if (-not (Test-Path $docker)) {
  Write-Error "Docker CLI missing: $docker"
}

foreach ($d in @(
  "D:\ClearPay\data\mssql",
  "D:\ClearPay\data\mysql",
  "D:\ClearPay\data\oracle"
)) {
  if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d -Force | Out-Null }
}

if (-not (Get-Process "Docker Desktop" -ErrorAction SilentlyContinue)) {
  Start-Process $desktop
}

$deadline = (Get-Date).AddMinutes(3)
do {
  & $docker info --format "{{.ServerVersion}}" 2>$null
  if ($LASTEXITCODE -eq 0) { break }
  Start-Sleep -Seconds 4
} while ((Get-Date) -lt $deadline)

if ($LASTEXITCODE -ne 0) {
  Write-Error "Docker engine still down. Reboot Windows (CBS RebootPending), then open Docker Desktop."
}

Set-Location $root
& $docker compose up -d
& $docker compose ps
