<#
.SYNOPSIS
  One mock 3D Secure card payment end to end against a running ClearPay web app, the way a merchant + browser would:
  create (merchant API) -> mock ACS page -> OTP -> signed callback -> capture -> optional partial refund.

.EXAMPLE
  ./scripts/demo-card-payment.ps1 -BaseUrl http://localhost:18080
  ./scripts/demo-card-payment.ps1 -BaseUrl http://localhost:18080 -OrderId fail-001      # notifications fault injection
  ./scripts/demo-card-payment.ps1 -BaseUrl http://localhost:18080 -Amount 10.37            # reconciliation amount difference

.NOTES
  Uses only the demo merchant key from appsettings.Development.json and the mock ACS OTP 123456. No real card data:
  4111111111111111 is the public Visa test number. Windows PowerShell 5.1 compatible.
#>
param(
    [string]$BaseUrl = "http://localhost:18080",
    [string]$MerchantKey = "dev-demo-shop-key",
    [decimal]$Amount = 120.00,
    [string]$Card = "4111111111111111",
    [string]$OrderId = ("ORD-" + [guid]::NewGuid().ToString("N").Substring(0, 10)),
    [decimal]$RefundAmount = 0,
    [switch]$SkipCapture
)

$ErrorActionPreference = "Stop"
$inv = [System.Globalization.CultureInfo]::InvariantCulture
$headers = @{ "X-Merchant-Key" = $MerchantKey }
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

function Merchant([string]$Method, [string]$Path, $Body) {
    $req = @{ Method = $Method; Uri = "$BaseUrl$Path"; Headers = $headers; ContentType = "application/json"; UseBasicParsing = $true }
    if ($null -ne $Body) { $req.Body = ($Body | ConvertTo-Json -Compress) }
    return Invoke-RestMethod @req
}

# 1) Merchant creates the payment. The gateway routes by BIN and always asks for 3DS.
$created = Merchant "POST" "/api/payments" @{
    merchantOrderId = $OrderId
    amount          = $Amount
    currency        = "TRY"
    cardNumber      = $Card
    returnUrl       = "/demo-magaza/sonuc"
}
Write-Host ("created  payment {0} order {1} provider {2} status {3}" -f $created.paymentId, $OrderId, $created.provider, $created.status)

# 2) Browser opens the mock bank ACS page and submits the OTP.
$acsUrl = "$BaseUrl$($created.redirectUrl)"
$page = Invoke-WebRequest -Uri $acsUrl -WebSession $session -UseBasicParsing
$token = [regex]::Match($page.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
if (-not $token) { $token = [regex]::Match($page.Content, 'value="([^"]+)"[^>]*name="__RequestVerificationToken"').Groups[1].Value }
$acs = Invoke-WebRequest -Uri $acsUrl -Method POST -WebSession $session -UseBasicParsing -Body @{
    Otp = "123456"; action = "confirm"; __RequestVerificationToken = $token
}

# 3) The ACS page auto-posts a signed form back to the gateway; we post the same hidden fields.
$fields = @{}
foreach ($m in [regex]::Matches($acs.Content, '<input type="hidden" name="(\w+)" value="([^"]*)"')) {
    if ($m.Groups[1].Value -ne "__RequestVerificationToken") { $fields[$m.Groups[1].Value] = [System.Net.WebUtility]::HtmlDecode($m.Groups[2].Value) }
}
Invoke-WebRequest -Uri "$BaseUrl/api/payments/3ds/callback" -Method POST -WebSession $session -UseBasicParsing -Body $fields | Out-Null
$payment = Merchant "GET" "/api/payments/$($created.paymentId)" $null
Write-Host ("3ds      status {0} reference {1}" -f $payment.status, $payment.providerReference)

if ($payment.status -eq "Authorized" -and -not $SkipCapture) {
    $payment = Merchant "POST" "/api/payments/$($created.paymentId)/capture" $null
    Write-Host ("capture  status {0} captured {1}" -f $payment.status, $payment.capturedAmount.ToString("F2", $inv))
}

if ($RefundAmount -gt 0 -and $payment.status -eq "Captured") {
    $payment = Merchant "POST" "/api/payments/$($created.paymentId)/refunds" @{ amount = $RefundAmount; reason = "demo" }
    Write-Host ("refund   status {0} refunded {1}" -f $payment.status, $payment.refundedAmount.ToString("F2", $inv))
}

[pscustomobject]@{ PaymentId = $created.paymentId; OrderId = $OrderId; Status = $payment.status; Provider = $payment.provider }
