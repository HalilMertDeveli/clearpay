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
GitHub Actions: `dotnet restore` → `build` → `test`. Secret yok.

## Canlı Q1
- Azure App Service (Linux) + Azure SQL
- Connection string App Settings
- Site internette açılır
- Redis/kuyruk broker şart değil

## Canlı Q2 (sonra)
- Azure Cache for Redis
- CloudAMQP (veya eşdeğeri)
- Kullanıcı aboneliğinde oluşturulur; ajan adımları listeler

## Yasak
- Repo’ya şifre, connection string, JWT signing key
- Kullanıcı yerine Azure / DNS hesabı açmak
