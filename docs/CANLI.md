# CANLI — ClearPay Q1 public URL

Kaynak: `docs/CALISMA-PLANI.md` **Faz 5**. Azure / DNS / ödeme hesabı **kullanıcı** açar; ajan şablon ve pipeline yazar.

**TASK-16 sıradaki tık (Halil).** Infra (`infra/main.bicep`, `deploy.ps1`) hazır. Ajan Portal / `az login` / DNS açmaz. Abonelik yokken URL uydurulmaz. Repo kökü **`D:\ClearPay\clearpay`**. Bu makinede `az` CLI yok (2026-08-17 tekrar) — liste doğrulanmadı. Yol: [`YOL.md`](YOL.md).

**Operasyon kimliği (T-025):** `halilmertdeveliii@gmail.com`. Azure / GitHub / Search Console / Ads bu kutudan. Ajan yeni hesap açmaz. Bu makinede `az` CLI **yok**. Gmail’de 2026-05-11 “Your new Azure account is ready” var. Secret git’e yok.

SEO/Ads: canlı URL **sonrası**; demo disclaimer (gerçek banka değil).

## Nerede

| Parça | Lokal Compose | Q1 Azure | Q2 (sonra) |
|--------|----------------|----------|------------|
| Web | host `dotnet run` :5153 | App Service **Linux**, .NET 8 | aynı |
| Identity | SQL Server `ClearPay` (AspNet*) | **Azure SQL** (`ConnectionStrings__ClearPay`) | aynı |
| Ledger | SQL Server `ClearPay` (Windows veya Docker) | **Azure SQL** (TASK-04+) | aynı |
| CI/CD | — | GitHub Actions: `main` → build/test; publish `AZURE_WEBAPP_NAME` doluysa | aynı |
| Kuyruk | RabbitMQ container (T-048 bağlı) | Hangfire in-process + outbox (`Hangfire__Enabled=true`) | CloudAMQP (`ConnectionStrings__RabbitMq`) |
| Cache | Redis container (T-041 bağlı) | yok (SQL özet) | Azure Cache for Redis |

**Bölge:** **West Europe**. Kaynak grubu: `rg-clearpay-weu`. Şablon: `infra/main.bicep`.

## Güzel URL (üç kademe)

App Service adı sırayla dene (küresel tekil):

1. `clearpay` → **https://clearpay.azurewebsites.net**
2. Doluysa `clearpay-wallet`
3. Doluysa `hm-clearpay` (Bicep varsayılanı)

Path: `/` özet, `/giris`, `/kayit`, `/havale`, `/yukle-cek`, `/hareketler`, `/admin`, `/api/health`.

Özel domain sonra: sen satın alırsın; CNAME → `<app>.azurewebsites.net`; HTTPS Azure managed cert. Ajan registrar açmaz.

## Senin tıklayacakların (sırayla)

1. Azure aboneliği (öğrenci veya pay-as-you-go). F1 denemek serbest. Ajan hesap açmaz.
2. [Azure CLI](https://aka.ms/installazurecliwindows) kur → PowerShell: **`az login`** (tarayıcı; `halilmertdeveliii@gmail.com`).
3. Repo kökü **`D:\ClearPay\clearpay`**: **`.\infra\deploy.ps1 -SqlAdminPassword (Read-Host -AsSecureString)`**  
   İsim doluysa `-WebAppName hm-clearpay`. Q2 Redis: `-IncludeQ2`. Script RG + Bicep + rastgele `Jwt__SigningKey` basar (yazdırılmaz).
4. Portal → App Service → **Environment variables** kontrol (değerleri sohbete yapıştırma):
   - Connection strings: ad **`ClearPay`**, tür **SQLAzure** (Bicep basar; ASP.NET bunu `ConnectionStrings:ClearPay` okur). Eski listede `ConnectionStrings__ClearPay` de geçerli.
   - `Jwt__SigningKey` (`deploy.ps1` basmış olmalı; 32+ karakter)
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `Hangfire__Enabled` = `true`
   - `Hangfire__UseMemoryStorage` = `false`
   - `Cors__Origins__0` = `https://<app>.azurewebsites.net` (Bicep / `deploy.ps1`)
5. Portal → App Service → **Get publish profile**. GitHub repo → Settings → Secrets → `AZURE_WEBAPP_PUBLISH_PROFILE`. Settings → Variables → `AZURE_WEBAPP_NAME` = web app adı.
6. GitHub Actions: workflow **Azure deploy** (veya `main` push; değişken boşsa job atlanır). Ajan push etmez.
7. Tarayıcı: script’in yazdığı `https://<app>.azurewebsites.net/api/health` sonra `/giris`. URL’yi ajan uydurmaz.
8. Q2 (şart değil): Portal `ConnectionStrings__Redis`. CloudAMQP hesabını **sen** açarsın; `ConnectionStrings__RabbitMq` yapıştır. Repo’ya yazılmaz.
9. Domain (isteğe bağlı): sen satın alırsın; CNAME → `<app>.azurewebsites.net`.

## Ajanın hazırladığı (hesap açmaz)

- `.github/workflows/ci.yml` — restore / build / test
- `.github/workflows/azure-deploy.yml` — `vars.AZURE_WEBAPP_NAME` doluysa zip deploy
- `infra/main.bicep` — plan F1 Linux, web .NET 8, Azure SQL Basic, Azure servis firewall
- `infra/q2.bicep` — Azure Cache for Redis Basic C0
- `infra/deploy.ps1` — RG + deployment + rastgele `Jwt__SigningKey` (yazdırılmaz)
- Production Identity: `UseSqlServer(ConnectionStrings:ClearPay)`. SQLite prod değil.
- Production hosting (T-095): `X-Forwarded-Proto` (App Service proxy), DataProtection keys `HOME/data-protection-keys`, cookie SameSite Lax, health `/api/health`.

## App Settings isimleri (değer yok)

Portal’da oluştur; **değerleri git’e koyma**.

| Ad | Ne | Ne zaman |
|----|-----|----------|
| Connection strings `ClearPay` (SQLAzure) | Azure SQL — ledger + Identity | Q1 (Bicep; `ConnectionStrings__ClearPay` de olur) |
| `Jwt__SigningKey` | API JWT (32+ rastgele bayt) | Q1 (`deploy.ps1`) |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Q1 |
| `Hangfire__Enabled` | `true` (in-process worker) | Q1 (Bicep) |
| `Hangfire__UseMemoryStorage` | `false` (Azure SQL storage) | Q1 (Bicep) |
| `Cors__Origins__0` | `https://<app>.azurewebsites.net` | Q1 (Bicep / `deploy.ps1`) |
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
TASK-15             GitHub Actions restore/build/test (hazır)
Kullanıcı           az login + .\infra\deploy.ps1 + GitHub secret + publish
TASK-16             açık URL = Done (şimdi blok: sen tıklarsın)
Q2                  Redis Bicep + CloudAMQP — TASK-16 şartı değil
```

TASK-16 kabul: tarayıcıda açık URL, giriş, boş/canlı özet. Redis şart değil.

## Ajanla paylaş (değer / şifre yok)

Portal veya script çıktısından **yalnız isim**. Connection string, SQL parolası, JWT, publish profile sohbete yapıştırma (GitHub Secret / Portal’de kalsın).

1. `az login` oldu mu? (bu Gmail)
2. Kaynak grubu (`rg-clearpay-weu` mi)
3. Web app adı (`clearpay` / `clearpay-wallet` / `hm-clearpay` / başka)
4. SQL sunucu adı (FQDN; şifre yok) — örn. `sql-….database.windows.net`
5. Environment variables **isim** listesi (Jwt, Hangfire, Cors, Connection strings ad `ClearPay`)
6. GitHub: `AZURE_WEBAPP_NAME` variable + `AZURE_WEBAPP_PUBLISH_PROFILE` secret **sen** koyarsın

Canlıda `admin@clearpay.test` **yok** (seed yalnız Development). İlk kullanıcı `/kayit`.

## Yasak

- Ajan Azure / DNS / domain / CloudAMQP / ödeme hesabı açmaz.
- Repo’ya connection string, JWT, SQL SA, publish profile.
- Canlıda lokal `ClearPay_Dev1!` veya SQLite `App_Data` (prod Identity Azure SQL).
- AWS, GCP, Kafka, Kubernetes (SPEC dışı).
