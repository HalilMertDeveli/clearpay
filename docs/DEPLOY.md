# DEPLOY — ClearPay

Hesap açma ajanın işi değildir. Ajan kodu ve şablonu hazırlar.

## Lokal

Compose: SQL Server + Redis + RabbitMQ. Web imajı yok; site host’ta `dotnet run` ile açılır (`http` profili, port 5153). Uygulama Redis/Rabbit’e **TASK-12’de** bağlanır; container’lar şimdiden ayağa kalkar.

```bash
docker compose up -d
docker compose ps
dotnet run --project src/ClearPay.Web --launch-profile http
```

Site: http://localhost:5153  
SQL: `localhost,1433` (SA). Identity lokal: SQLite `App_Data`. Ledger SQL TASK-04.  
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

Sifreler `.env` (gitignore). `.env.example` placeholder. App connection string **MSSQL only** (`localhost,1433`, SA). Identity lokal: SQLite `App_Data`.

```bash
docker compose up -d
docker compose -f docker-compose.databases.yml up -d
docker compose ps
docker compose -f docker-compose.databases.yml ps
powershell -File scripts/db-smoke.ps1
```

Windows native MySQL84 zaten `:3306` dinliyorsa Compose MySQL icin o servisi durdur (veriyi silme). Native MSSQLSERVER data C: Program Files'da kalir; Compose SQL D: bind kullanir.

Docker Desktop Linux motoru Virtual Machine Platform ister. `wsl --install --no-distribution` sonrasi **reboot** sart (CBS pending). Oracle EULA `ORACLE_PASSWORD` ile kabul; pirated imaj yok. Redis/Rabbit `docker-compose.yml` icinde durur (TASK-12).
## CI (TASK-15)

`.github/workflows/ci.yml`: `dotnet restore` → `build` → `test`. Secret yok.

## Canlı Q1

Tam tıklama: **`docs/CANLI.md`**. Özet:

- West Europe, App Service Linux + Azure SQL. Hangfire in-process.
- Şablon: `infra/main.bicep`. Kullanıcı: `az login` sonra `.\infra\deploy.ps1`.
- Publish: GitHub secret `AZURE_WEBAPP_PUBLISH_PROFILE` + variable `AZURE_WEBAPP_NAME`. Workflow `azure-deploy.yml` değişken boşsa atlar.
- Path: `/`, `/giris`, `/kayit`, `/havale`, `/yukle-cek`, `/hareketler`, `/admin`, `/api/health`.
- Production Identity Azure SQL. SQLite prod değil.

## Canlı Q2 (sonra)

- Azure Cache for Redis: `infra/q2.bicep` (`deploy.ps1 -IncludeQ2`). Anahtarı portalden `ConnectionStrings__Redis`.
- CloudAMQP: kullanıcı kaydı; `ConnectionStrings__RabbitMq`. Ajan hesap açmaz.
- Uygulama bağlantısı TASK-12.

## Yasak

- Repo’ya şifre, connection string, JWT signing key, publish profile
- Kullanıcı yerine Azure / DNS / CloudAMQP hesabı açmak
