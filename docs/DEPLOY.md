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

ClearPay ledger SQL Server kalir. Asagidaki motorlar **lokal test** icindir; cok-veritabani uygulamasi **sonra** (SPEC 8 ekran sabit, yeni urun ekrani yok).

| Motor | Nasil | Port | Kullanici (lokal demo) |
|-------|--------|------|-------------------------|
| SQL Server | docker compose up -d servis sql **veya** Windows native (MSSQLSERVER) | 1433 | sa / .env MSSQL_SA_PASSWORD (Docker). Native: Windows auth (sqlcmd -S localhost -E) |
| MySQL 8 | docker compose -f docker-compose.databases.yml up -d | 3306 | oot / MYSQL_ROOT_PASSWORD, DB ClearPay |
| Oracle XE 21 | ayni databases compose (gvenzl/oracle-xe:21-slim) | 1521 | APP_USER clearpay, service XEPDB1. Ilk acilis 5-15 dk, ~2 GB RAM. |

`ash
docker compose -f docker-compose.databases.yml up -d
powershell -File scripts/db-smoke.ps1
dotnet test ClearPay.slnx
`

Docker Desktop ilk kurulumda WSL2 + VirtualMachinePlatform + Hyper-V (cikis 3010) **reboot** ister. Oracle EULA imaj degiskeni ORACLE_PASSWORD ile kabul edilir; pirated imaj yok. docker-compose.yml (SQL/Redis/Rabbit) bu dosyaya dokunulmaz.

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
