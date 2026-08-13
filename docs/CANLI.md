# CANLI — ClearPay Q1 public URL

Kaynak: `docs/CALISMA-PLANI.md` **Faz 5**. Azure / DNS / ödeme hesabı **kullanıcı** açar; ajan şablon ve pipeline yazar.

**Orchestrator:** TASK-16 Done yalnızca tarayıcıda açık URL. Abonelik yokken ajan Portal açmaz. TASK-15 = GitHub Actions `dotnet test`. `dotnet test` kırmızıysa Actions Done yok.

**Operasyon kimliği (T-025):** `halilmertdeveliii@gmail.com`. Azure / GitHub / Search Console / Ads bu kutudan. Ajan yeni hesap açmaz. Bu makinede `az` CLI **yok** — abonelik listesi doğrulanmadı, uydurulmadı. Gmail’de 2026-05-11 “Your new Azure account is ready” var; 2025-04 ücretsiz deneme uyarı maili de var (PAYG yükseltme / silinme). **TASK-16 şimdi değil** — deploy TASK-16 ajanına. Secret git’e yok.

SEO/Ads: canlı URL **sonrası**; demo disclaimer (gerçek banka değil).

## Nerede

| Parça | Lokal Compose | Q1 Azure | Q2 (sonra) |
|--------|----------------|----------|------------|
| Web | host `dotnet run` :5153 | App Service **Linux**, .NET 8 | aynı |
| Identity | SQLite `App_Data` | **Azure SQL** (`ConnectionStrings__ClearPay`) | aynı |
| Ledger | Docker SQL Server | **Azure SQL** (TASK-04+) | aynı |
| CI/CD | — | GitHub Actions: `main` → build/test; publish `AZURE_WEBAPP_NAME` doluysa | aynı |
| Kuyruk | RabbitMQ container (bağlı değil) | Hangfire in-process + outbox | CloudAMQP (`ConnectionStrings__RabbitMq`) |
| Cache | Redis container (bağlı değil) | yok | Azure Cache for Redis |

**Bölge:** **West Europe**. Kaynak grubu: `rg-clearpay-weu`. Şablon: `infra/main.bicep`.

## Güzel URL (üç kademe)

App Service adı sırayla dene (küresel tekil):

1. `clearpay` → **https://clearpay.azurewebsites.net**
2. Doluysa `clearpay-wallet`
3. Doluysa `hm-clearpay` (Bicep varsayılanı)

Path: `/` özet, `/giris`, `/kayit`, `/havale`, `/yukle-cek`, `/hareketler`, `/admin`, `/api/health`.

Özel domain sonra: sen satın alırsın; CNAME → `<app>.azurewebsites.net`; HTTPS Azure managed cert. Ajan registrar açmaz.

## Senin tıklayacakların

1. Azure aboneliği (öğrenci veya pay-as-you-go). F1 denemek serbest.
2. [Azure CLI](https://aka.ms/installazurecliwindows) → `az login`
3. Repo kökünde: `.\infra\deploy.ps1 -SqlAdminPassword (Read-Host -AsSecureString)`  
   İsim doluysa `-WebAppName hm-clearpay`. Q2 Redis: `-IncludeQ2`.
4. Portal → App Service → **Get publish profile**. GitHub repo → Settings → Secrets → `AZURE_WEBAPP_PUBLISH_PROFILE`. Variables → `AZURE_WEBAPP_NAME`.
5. GitHub Actions: **Azure deploy** workflow (veya `main` push; değişken boşsa job atlanır).
6. Tarayıcı: `https://<app>.azurewebsites.net/api/health` sonra `/giris`.
7. Q2 kuyruk: [CloudAMQP](https://www.cloudamqp.com/) hesabını **sen** açarsın; URL’yi `ConnectionStrings__RabbitMq` olarak yapıştır. Repo’ya yazılmaz.
8. Domain (isteğe bağlı): DNS CNAME + App Service custom domain.

## Ajanın hazırladığı (hesap açmaz)

- `.github/workflows/ci.yml` — restore / build / test
- `.github/workflows/azure-deploy.yml` — `vars.AZURE_WEBAPP_NAME` doluysa zip deploy
- `infra/main.bicep` — plan F1 Linux, web .NET 8, Azure SQL Basic, Azure servis firewall
- `infra/q2.bicep` — Azure Cache for Redis Basic C0
- `infra/deploy.ps1` — RG + deployment + rastgele `Jwt__SigningKey` (yazdırılmaz)
- Production Identity: `UseSqlServer(ConnectionStrings:ClearPay)`. SQLite prod değil.

## App Settings isimleri (değer yok)

Portal’da oluştur; **değerleri git’e koyma**.

| Ad | Ne | Ne zaman |
|----|-----|----------|
| `ConnectionStrings__ClearPay` | Azure SQL (ledger + Identity) | Q1 (Bicep basar) |
| `Jwt__SigningKey` | API JWT (32+ rastgele bayt) | Q1 (`deploy.ps1`) |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Q1 |
| `Hangfire__WorkerEnabled` | `true` (in-process) | Q1 |
| `ConnectionStrings__Redis` | `host:6380,ssl=true,password=…` | Q2 |
| `ConnectionStrings__RabbitMq` | CloudAMQP `amqps://…` | Q2 |

Lokal Compose eşleri (Azure’da kullanılmaz): SQL `ClearPay_Dev1!`, Redis `localhost:6379`, Rabbit `amqp://guest:guest@localhost:5672/`.

## Maliyet (uyarı)

| Kaynak | SKU | Not |
|--------|-----|-----|
| App Service | **F1** ücretsiz | Always On yok — ilk istek 10–30 sn. |
| App Service | **B1** | Always On; ücretli. |
| Azure SQL | **Basic** | Bicep varsayılanı. |
| Redis C0 | Q2 | `-IncludeQ2`; birkaç dolar. |
| CloudAMQP | Q2 | Azure dışı; sen kaydolursun. |

F1 soğuk başlama: `/api/health` ile ısındır.

## Sıra

```
TASK-15             GitHub Actions restore/build/test (bu şablon)
Kullanıcı           az login + .\infra\deploy.ps1 + GitHub secret
TASK-16             açık URL = Done
Q2                  Redis Bicep + CloudAMQP — TASK-16 şartı değil
TASK-12             uygulama Redis/Rabbit’e bağlanır (şimdi yalnızca container)
```

TASK-16 kabul: tarayıcıda açık URL, giriş, boş/canlı özet. Redis şart değil.

## Yasak

- Ajan Azure / DNS / domain / CloudAMQP / ödeme hesabı açmaz.
- Repo’ya connection string, JWT, SQL SA, publish profile.
- Canlıda lokal `ClearPay_Dev1!` veya SQLite `App_Data`.
- AWS, GCP, Kafka, Kubernetes (SPEC dışı).
