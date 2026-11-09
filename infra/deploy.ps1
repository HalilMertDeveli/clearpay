#Requires -Version 5.1
<#
  Creates Q1 Azure resources (App Service Linux + Azure SQL). Does not open an Azure account.
  You must already have a subscription and be logged in: az login

  Example:
    .\infra\deploy.ps1 -SqlAdminPassword (Read-Host -AsSecureString)
    .\infra\deploy.ps1 -WebAppName hm-clearpay -IncludeQ2
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [SecureString] $SqlAdminPassword,
    [string] $WebAppName = "hm-clearpay",
    [string] $ResourceGroup = "rg-clearpay-weu",
    [string] $Location = "westeurope",
    [string] $SqlAdminLogin = "clearpayadmin",
    [switch] $IncludeQ2
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    throw "Azure CLI (az) is required. Install https://aka.ms/installazurecliwindows then run az login."
}

$account = az account show --output json 2>$null
if (-not $account) {
    throw "Not logged in. Run: az login"
}

$bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SqlAdminPassword)
try {
    $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

if ([string]::IsNullOrWhiteSpace($plain) -or $plain.Length -lt 8) {
    throw "SQL admin password must be at least 8 characters (upper, lower, digit)."
}

Write-Host "Creating resource group $ResourceGroup in $Location ..."
az group create --name $ResourceGroup --location $Location | Out-Null

$deployQ2 = if ($IncludeQ2) { "true" } else { "false" }
Write-Host "Deploying Bicep (web=$WebAppName, Q2 Redis=$deployQ2) ..."
az deployment group create `
    --resource-group $ResourceGroup `
    --template-file (Join-Path $here "main.bicep") `
    --parameters webAppName=$WebAppName sqlAdminLogin=$SqlAdminLogin sqlAdminPassword=$plain deployQ2=$deployQ2 `
    --output table

$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$jwt = [Convert]::ToBase64String($bytes)

$url = az webapp show --resource-group $ResourceGroup --name $WebAppName --query defaultHostName --output tsv

Write-Host "Setting Jwt__SigningKey and Cors origin (values not printed) ..."
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings "Jwt__SigningKey=$jwt" "Cors__Origins__0=https://$url" `
    | Out-Null

$plain = $null
[GC]::Collect()
Write-Host ""
Write-Host "Q1 site: https://$url"
Write-Host "Health:  https://$url/api/health"
Write-Host "Login:   https://$url/giris"
Write-Host ""
Write-Host "GitHub: Settings → Secrets → AZURE_WEBAPP_PUBLISH_PROFILE (App Service → Get publish profile)"
Write-Host "GitHub: Settings → Variables → AZURE_WEBAPP_NAME = $WebAppName"
Write-Host "Then run workflow Azure deploy, or push main."
if ($IncludeQ2) {
    Write-Host "Q2 Redis host is in the deployment output. Add the portal primary key to ConnectionStrings__Redis."
    Write-Host "CloudAMQP: create an instance yourself, paste ConnectionStrings__RabbitMq in App Settings. Do not commit it."
}
