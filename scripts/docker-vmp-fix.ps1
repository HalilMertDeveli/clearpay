$ErrorActionPreference = "Continue"
$log = "C:\Users\clt\Projects\clearpay\scripts\docker-vmp-fix.log"
function L($m) { $line = "$(Get-Date -Format o) $m"; Add-Content -Path $log -Value $line; Write-Host $line }

L "=== Elevated Docker/VMP fix start ==="
L "whoami=$(whoami)"
try { $id = [Security.Principal.WindowsIdentity]::GetCurrent(); $p = New-Object Security.Principal.WindowsPrincipal($id); L "IsAdmin=$($p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))" } catch { L "admin-check-fail $_" }

L "--- bcdedit before ---"
$bcd = bcdedit 2>&1 | Out-String
L $bcd
L "Setting hypervisorlaunchtype auto"
$bcdSet = bcdedit /set hypervisorlaunchtype auto 2>&1 | Out-String
L "bcdedit set exit=$LASTEXITCODE output=$bcdSet"

L "--- DISM enable features (norestart) ---"
$features = @(
  "VirtualMachinePlatform",
  "Microsoft-Windows-Subsystem-Linux",
  "Microsoft-Hyper-V-All",
  "Containers"
)
foreach ($f in $features) {
  L "Enabling $f"
  $out = dism.exe /online /enable-feature /featurename:$f /all /norestart 2>&1 | Out-String
  L "DISM $f exit=$LASTEXITCODE"
  L $out
}

L "--- Enable-WindowsOptionalFeature ---"
foreach ($f in @("VirtualMachinePlatform","Microsoft-Windows-Subsystem-Linux","Microsoft-Hyper-V-All","Containers")) {
  try {
    $r = Enable-WindowsOptionalFeature -Online -FeatureName $f -All -NoRestart -ErrorAction Stop
    L "EWO $f RestartNeeded=$($r.RestartNeeded) State=$($r.State)"
  } catch {
    L "EWO $f error: $_"
  }
}

L "--- wsl install/update ---"
$w1 = wsl.exe --install --no-distribution --no-launch 2>&1 | Out-String
L "wsl --install exit=$LASTEXITCODE $w1"
$w2 = wsl.exe --update 2>&1 | Out-String
L "wsl --update exit=$LASTEXITCODE $w2"
$w3 = wsl.exe --set-default-version 2 2>&1 | Out-String
L "wsl --set-default-version 2 exit=$LASTEXITCODE $w3"
$w4 = wsl.exe --status 2>&1 | Out-String
L "wsl --status exit=$LASTEXITCODE $w4"

L "--- feature states after ---"
foreach ($f in $features) {
  try {
    $st = Get-WindowsOptionalFeature -Online -FeatureName $f
    L "Feature $f = $($st.State)"
  } catch { L "Feature $f query fail $_" }
}

L "--- reboot pending keys ---"
$rp1 = Test-Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending"
$rp2 = Test-Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired"
L "CBS.RebootPending=$rp1 WU.RebootRequired=$rp2"

L "=== Elevated Docker/VMP fix done ==="
