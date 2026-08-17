# DEPLOY — ClearPay

Hesap açma ajanın işi değildir. Ajan kodu ve şablonu hazırlar.

## Lokal

Compose: SQL Server + Redis + RabbitMQ. Web imajı yok; site host’ta `dotnet run` ile açılır (`http` profili, port 5153). Uygulama Redis özet cache + Rabbit `clearpay.outbox` (T-041 / T-048). Connection string boş veya broker düşer → SQL / log yedek.

```bash
docker compose up -d
docker compose ps
dotnet run --project src/ClearPay.Web --launch-profile http
```

Site: http://localhost:5153  
SQL: Development LocalDB `(localdb)\MSSQLLocalDB` / `ClearPay` (Identity + ledger, T-058 / T-076). Docker SQL `localhost,1433` (SA) Compose yedek; Production Azure SQL.  
Redis: `localhost:6379`. Rabbit yönetim: http://localhost:15672 (`guest` / `guest`).  
Lokal SA şifresi varsayılanı `ClearPay_Dev1!` (yalnızca Docker; Azure’da kullanma). `.env.example` kopyalanabilir; `.env` commit edilmez.

## Lokal ek motorlar (MySQL / Oracle)

ClearPay ledger **yalnizca SQL Server** kalir (SPEC). MySQL ve Oracle lokal Compose yan servistir; Web/Identity tasinmaz. Cok-veritabani uygulamasi **sonra** (8 ekran sabit).

Veri bind mount (T-021), C: named volume silinmez:

| Motor | Compose | Port | Data |
|-------|---------|------|------|
| SQL Server | `docker compose up -d` (servis sql) | 1433 | `D:\ClearPay\data\mssql` |
| MySQL 8.4 | `docker compose -f docker-compose.databases.yml up -d` | 3306 | `D:\ClearPay\data\mysql` |
| Oracle XE 21 | ayni databases compose (`gvenzl/oracle-xe:21-slim`) | 1521 | `D:\ClearPay\data\oracle` |

Sifreler `.env` (gitignore). `.env.example` placeholder. App connection string **MSSQL only**. Development: LocalDB `ClearPay` (Identity + ledger). Testler SQLite (`ClearPay:UseSqliteLedger`).

```bash
docker compose up -d
docker compose -f docker-compose.databases.yml up -d
docker compose ps
docker compose -f docker-compose.databases.yml ps
powershell -File scripts/db-smoke.ps1
```

Windows native MySQL84 zaten `:3306` dinliyorsa Compose MySQL icin o servisi durdur (veriyi silme). Native: `Get-Service MySQL84` / `net start MySQL84`. Development `ConnectionStrings:MySql` yan motor (T-077); `AddClearPay` / Identity LocalDB veya SQL Server kalir. Flutter mysql paketi yok. Native MSSQLSERVER data C: Program Files'da kalir; Compose SQL D: bind kullanir.

Docker Desktop Linux motoru Virtual Machine Platform ister. Ozellikler acildi (`scripts/docker-vmp-fix.ps1`); **CBS reboot pending** — ajan reboot etmez. Reboot sonrasi:

```powershell
powershell -File scripts/docker-up.ps1
```

Development (`dotnet run` / VS `http`): native SQL **shared memory** + Windows auth (`Server=localhost`, Integrated Security). `localhost,1433` + `sa` Compose SQL icindir; native TCP kapali ki reboot sonrasi 1433 Docker'a kalsin.

Oracle EULA `ORACLE_PASSWORD` ile kabul; pirated imaj yok. Redis/Rabbit `docker-compose.yml` icinde durur (TASK-12).
## CI (TASK-15)

`.github/workflows/ci.yml`: `dotnet restore` → `build` → `test`. Secret yok.

## Canlı Q1

Tam tıklama: **`docs/CANLI.md`** (T-104). Özet:

- **Canlı kök:** https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net
- App Service adı **`ClearPay`** (GitHub `AZURE_WEBAPP_NAME`). RG **`ClearPay_group`**. Canada Central. Linux .NET 8, PremiumV2.
- Hangfire in-process (`Hangfire__Enabled=true`, SQL storage). Identity + ledger = `ConnectionStrings:ClearPay`.
- `infra/main.bicep` / `deploy.ps1` **unused** — mevcut siteyi ezme.
- Publish: secret `AZURE_WEBAPP_PUBLISH_PROFILE` (Halil Portal **Get publish profile**). Workflow `azure-deploy.yml` `main` push veya `workflow_dispatch`.
- Path: `/`, `/giris`, `/kayit`, `/havale`, `/yukle-cek`, `/hareketler`, `/admin`, `/kartlar`, `/api/health`.
- Production Identity Azure SQL. SQLite prod değil. Production CORS = canlı https origin.

## Canlı Q2 (sonra)

- Azure Cache for Redis: `infra/q2.bicep` (`deploy.ps1 -IncludeQ2`). Anahtarı portalden `ConnectionStrings__Redis`.
- CloudAMQP: kullanıcı kaydı; `ConnectionStrings__RabbitMq`. Ajan hesap açmaz.
- Uygulama bağlandı (T-041 / T-048). Portal’e `ConnectionStrings__Redis` / `ConnectionStrings__RabbitMq` sen yapıştırırsın.

## Yasak

- Repo’ya şifre, connection string, JWT signing key, publish profile
- Kullanıcı yerine Azure / DNS / CloudAMQP hesabı açmak
