# DEPLOY — ClearPay

Hesap açma ajanın işi değildir. Ajan kodu ve talimatı hazırlar.

## Lokal
Compose şu an yalnızca SQL Server ayağa kaldırır. Web imajı yok; site host’ta `dotnet run` ile açılır (`http` profili, port 5153). `.dockerignore` `bin/` ve `obj/` dışlar (ileride web imajı için).

```bash
docker compose up -d
docker compose ps
dotnet run --project src/ClearPay.Web --launch-profile http
```
Site: http://localhost:5153  
SQL: `localhost,1433` (SA). Uygulama TASK-04’e kadar SQL’e bağlanmaz. Redis + RabbitMQ TASK-12.  
Lokal SA şifresi varsayılanı `ClearPay_Dev1!` (yalnızca Docker; Azure’da kullanma). `.env.example` kopyalanabilir; `.env` commit edilmez.

## CI
GitHub Actions (TASK-15): `dotnet restore` → `build` → `test`. Secret yok. `main` → publish **TASK-16**; şimdi yok.

## Canlı Q1
Tam plan: **`docs/CANLI.md`**. Özet:
- **West Europe**, App Service Linux + Azure SQL. Hangfire in-process OK. Redis/Rabbit Q2.
- Ücretsiz URL: `https://clearpay.azurewebsites.net` (doluysa `clearpay-wallet` / `hm-clearpay`).
- Path: `/`, `/giris`, `/kayit`, `/havale`, `/yukle-cek`, `/hareketler`, `/admin`, `/api/...`.
- Connection string App Settings; repo’ya yazılmaz.
- TASK-16 **kullanıcı Azure aboneliği açmadan başlamaz.**

## Canlı Q2 (sonra)
- Azure Cache for Redis
- CloudAMQP (veya eşdeğeri)
- Kullanıcı aboneliğinde oluşturulur; ajan adımları listeler

## Yasak
- Repo’ya şifre, connection string, JWT signing key
- Kullanıcı yerine Azure / DNS hesabı açmak
