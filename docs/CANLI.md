# CANLI — ClearPay Q1 public URL

Kaynak: `docs/CALISMA-PLANI.md` **Faz 5**. Azure / DNS / ödeme hesabı **kullanıcı** açar; ajan şablon ve pipeline yazar.

**T-104 kilit (2026-08-17).** Halil Portal’de Linux App Service açtı. Ajan `az login` / abonelik / DNS açmaz. Secret git’e yok. Bicep (`infra/deploy.ps1`) **bu siteyi ezmez** — şablon durur, unused. Yol: [`YOL.md`](YOL.md).

**Operasyon kimliği (T-025):** `halilmertdeveliii@gmail.com`. Azure / GitHub / Search Console / Ads bu kutudan. Ajan yeni hesap açmaz.

SEO/Ads: canlı URL **sonrası**; demo disclaimer (gerçek banka değil). Canlıda `admin@clearpay.test` **yok** (seed yalnız Development). İlk kullanıcı `/kayit`.

## Şu an (Portal ARM — uydurma yok)

| Alan | Değer |
|------|--------|
| Subscription | `c706e53f-1b53-4baa-99fd-e09b63ef8684` |
| Resource group | **`ClearPay_group`** (Bicep varsayılanı `rg-clearpay-weu` **değil**) |
| App Service adı | **`ClearPay`** ← GitHub variable `AZURE_WEBAPP_NAME` |
| Kind | `app,linux` |
| Bölge | **Canada Central** (T-005 West Europe varsayılanı bu siteyi değiştirmez) |
| Runtime | `DOTNETCORE\|8.0` |
| SKU | PremiumV2 (`ASP-ClearPaygroup-8f4e`) |
| State | Running |
| Canlı kök | **https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net** |
| SCM | `clearpay-eecuaqc7c5ehbmb5.scm.canadacentral-01.azurewebsites.net` |
| VNet | `ClearPayVnet` / `ClearPayAppSubnet` |

Path: `/` özet, `/giris`, `/kayit`, `/havale`, `/yukle-cek`, `/hareketler`, `/admin`, `/kartlar`, `/api/health`.

**Health (2026-08-17 fetch):** `GET /api/health` ve `/giris` **404** — App Service boş (zip yok). TASK-16 **Done değil**.

Özel domain sonra: sen satın alırsın; CNAME → bu `defaultHostName`; HTTPS Azure managed cert. Ajan registrar açmaz.

## Nerede

| Parça | Lokal Compose | Q1 Azure (bu site) | Q2 (sonra) |
|--------|----------------|----------|------------|
| Web | host `dotnet run` :5153 | App Service **Linux**, .NET 8, zip via GitHub Actions | aynı |
| Identity | SQL Server `ClearPay` (AspNet*) | **Azure SQL** (`ConnectionStrings:ClearPay`) | aynı |
| Ledger | SQL Server `ClearPay` | **Azure SQL** | aynı |
| CI/CD | — | GitHub Actions: `main` → `azure-deploy.yml` (`AZURE_WEBAPP_NAME` dolu) | aynı |
| Kuyruk | RabbitMQ container (T-048 bağlı) | Hangfire in-process + outbox (`Hangfire__Enabled=true`) | CloudAMQP |
| Cache | Redis container (T-041 bağlı) | yok (SQL özet) | Azure Cache for Redis |

## GitHub ↔ Azure

| GitHub | Değer | Kim |
|--------|--------|-----|
| Actions **variable** `AZURE_WEBAPP_NAME` | `ClearPay` | Ajan 2026-08-17 `gh variable set` |
| Actions **secret** `AZURE_WEBAPP_PUBLISH_PROFILE` | Portal XML | **Halil** — uydurulmaz |
| Workflow | `.github/workflows/azure-deploy.yml` | `workflow_dispatch` + **push `main`** |

Deploy-on-push **`main`** dalında. Feature dalı zip atmaz; merge/push `main` veya Actions → **Azure deploy** → Run workflow (`main`).

Publish profile sohbete / git’e yapıştırma.

## Kalan Halil tıkları (secret + Portal; `az login` şart değil)

1. Portal → Resource group **`ClearPay_group`** → App Service **`ClearPay`** → **Get publish profile** (Download).  
   GitHub `HalilMertDeveli/clearpay` → **Settings → Secrets and variables → Actions → New repository secret**  
   Name: **`AZURE_WEBAPP_PUBLISH_PROFILE`**  
   Value: indirilen `.PublishSettings` dosyasının **tüm XML** içeriği. Sohbete yapıştırma.
2. Portal → Configuration → **General settings**:
   - **Startup Command:** `dotnet ClearPay.Web.dll` (`appCommandLine` ARM’de null; Linux zip bunu ister)
   - **HTTPS Only:** **On** (ARM `httpsOnly: false`)
   - Health check path (isteğe): `/api/health`
3. Portal → **Environment variables** (değerleri sohbete yapıştırma):
   - Connection strings: ad **`ClearPay`**, tür **SQLAzure** → `ConnectionStrings:ClearPay`
   - `Jwt__SigningKey` (32+ karakter)
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `Hangfire__Enabled` = `true`
   - `Hangfire__UseMemoryStorage` = `false`
   - `Cors__Origins__0` = `https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net` (kod da aynı origin; Portal yedek)
4. Azure SQL firewall: VNet `ClearPayVnet` / `ClearPayAppSubnet` kullanıyorsan SQL’i o ağa bağla; SQL public ise **Allow Azure services**. Ajan Portal açmaz.
5. Secret kaydı + CORS/JWT/SQL ayarı bitince: GitHub Actions → **Azure deploy** → Run workflow (**`main`**). Veya bu dalı `main`’e merge edip push.
6. Tarayıcı: https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net/api/health sonra `/giris`. 200 olunca TASK-16 Done.
7. Q2 (şart değil): `ConnectionStrings__Redis`. CloudAMQP’yi **sen** açarsın; `ConnectionStrings__RabbitMq`. Repo’ya yazılmaz.
8. Domain (isteğe bağlı): sen satın alırsın; CNAME → `clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net`.

**Yapma:** `.\infra\deploy.ps1` bu RG üzerine. FTP kullanıcı/parola git’e yok.

## Ajanın hazırladığı (hesap açmaz)

- `.github/workflows/ci.yml` — restore / build / test
- `.github/workflows/azure-deploy.yml` — `vars.AZURE_WEBAPP_NAME` doluysa zip deploy
- `infra/main.bicep` / `deploy.ps1` — **unused** (West Europe şablon; canlı site Portal)
- Production Identity: `UseSqlServer(ConnectionStrings:ClearPay)`. SQLite prod değil.
- Production hosting (T-095): `X-Forwarded-Proto`, DataProtection `HOME/data-protection-keys`, cookie SameSite Lax, health `/api/health`.
- Production CORS (T-104): `https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net`

## App Settings isimleri (değer yok)

Portal’da oluştur; **değerleri git’e koyma**.

| Ad | Ne | Ne zaman |
|----|-----|----------|
| Connection strings `ClearPay` (SQLAzure) | Azure SQL — ledger + Identity | Q1 |
| `Jwt__SigningKey` | API JWT (32+ rastgele bayt) | Q1 |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Q1 |
| `Hangfire__Enabled` | `true` (in-process worker) | Q1 |
| `Hangfire__UseMemoryStorage` | `false` (Azure SQL storage) | Q1 |
| `Cors__Origins__0` | `https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net` | Q1 |
| `ConnectionStrings__Redis` | `host:6380,ssl=true,password=…` | Q2 |
| `ConnectionStrings__RabbitMq` | CloudAMQP `amqps://…` | Q2 |

Lokal Compose eşleri (Azure’da kullanılmaz): SQL `ClearPay_Dev1!`, Redis `localhost:6379`, Rabbit `amqp://guest:guest@localhost:5672/`.

## Maliyet (uyarı)

Bu canlı site **PremiumV2** (F1 değil). Always On ARM’de false — soğuk başlama olabilir; `/api/health` ile ısındır.

| Kaynak | Not |
|--------|-----|
| App Service PremiumV2 | Halil’in Portal SKU’su |
| Azure SQL | Firewall / VNet Halil |
| Redis C0 | Q2; Bicep `q2.bicep` unused |
| CloudAMQP | Q2; sen kaydolursun |

## Sıra

```
TASK-15             GitHub Actions restore/build/test (hazır)
T-104               canlı host kilit + AZURE_WEBAPP_NAME=ClearPay
Halil               Get publish profile → secret; startup + HTTPS Only; SQL/JWT
main push / dispatch  zip deploy
TASK-16 Done        /api/health 200 + /giris açılır
Q2                  Redis / CloudAMQP — TASK-16 şartı değil
```

TASK-16 kabul: tarayıcıda açık URL, giriş (`/kayit` ile; Production seed yok), boş/canlı özet. Redis şart değil.

## Ajanla paylaş (değer / şifre yok)

Connection string, SQL parolası, JWT, publish profile sohbete yapıştırma.

1. Secret `AZURE_WEBAPP_PUBLISH_PROFILE` GitHub’da var mı? (şu an yoktu)
2. Portal startup `dotnet ClearPay.Web.dll` ve HTTPS Only On
3. App Settings **isim** listesi (değer yok)
4. `/api/health` 200 olduktan sonra söyle

## Yasak

- Ajan Azure / DNS / domain / CloudAMQP / ödeme hesabı açmaz.
- Repo’ya connection string, JWT, SQL SA, publish profile, FTP parolası.
- Canlıda lokal `ClearPay_Dev1!` veya SQLite `App_Data`.
- `az login` / `deploy.ps1` ile mevcut `ClearPay` sitesini yeniden yaratmak.
- AWS, GCP, Kafka, Kubernetes (SPEC dışı).
