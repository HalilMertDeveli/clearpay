@echo off
REM T-065: Halil runs this in Command Prompt after firebase login.
REM Does not create a Google account. Does not move the ledger off SQL Server.
setlocal
cd /d "%~dp0.."

where flutter >nul 2>&1
if errorlevel 1 (
  echo flutter not on PATH. Use Command Prompt, not PowerShell.
  exit /b 1
)

dart pub global activate flutterfire_cli
if errorlevel 1 exit /b 1

where firebase >nul 2>&1
if errorlevel 1 (
  echo Firebase CLI missing. Install: npm install -g firebase-tools
  echo Then: firebase login   ^(halilmertdeveliii@gmail.com^)
  echo Then re-run this script.
  exit /b 1
)

firebase projects:list
if errorlevel 1 (
  echo Not logged in. Run: firebase login
  exit /b 1
)

dart pub global run flutterfire_cli:flutterfire configure --platforms=android,ios,windows --yes --overwrite-firebase-options
if errorlevel 1 exit /b 1

echo Firebase options written. JWT + SQL ledger unchanged. Commit firebase_options.dart and google-services.json when ready.
endlocal
