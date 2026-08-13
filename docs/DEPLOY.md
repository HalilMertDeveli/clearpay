# DEPLOY — ClearPay

Hesap açma ajanın işi değildir. Ajan kodu ve talimatı hazırlar.

## Lokal
```bash
docker compose up -d
dotnet run --project src/ClearPay.Web
```
Compose: SQL Server (gün 1). Redis + RabbitMQ TASK-12.

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
