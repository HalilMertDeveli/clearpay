# ClearPay

[English](README.md) | **Türkçe** | [Français](README.fr.md)

[![CI](https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml/badge.svg)](https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml)
[![MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

ASP.NET Core 8 cüzdan. Tek host: site Razor Pages, API JSON. SQL Server Docker’da. Çift kayıt Domain’de — `Wallet` üzerinde `Balance` kolonu yok.

Ben Halil Mert Develi. Bunu Papara klonu diye değil, .NET mülakatında (Intertech / Softtech kapısı) savunabileceğim repo diye yazdım. Arayüz Türkçe. Lisans MIT.

Demo. Yükle/çek için sahte `IBankGateway`. E-para lisansı yok, FAST yok, kart tahsilatı yok, açık Azure URL yok. Bunu banka diye anlatmayın; footer zaten söylüyor.

## Defter

Alışılmış öğrenci cüzdanı: `wallet.Balance -= amount; SaveChanges();`. İz kalmaz, yarışta last-write-wins, iade edilecek çift yoktur. Freeze bir sayıya bantlanmış bayrak olur.

Burada her hareket bir `LedgerEntry` çifti: debit (−) ve credit (+), aynı `PairId`, aynı `CorrelationId`, tutarlar toplamı sıfır. Bakiye `LedgerPair.NetOf`. `Wallet`’ta bakiye alanı yok. İade = ters çift; eski satır silinmez. `UPDATE Balance` yardımcısı yok — kasten.

Aynı `Idempotency-Key` = aynı niyet (çift tıklama, proxy retry). İlk başarı `201`. Tekrar `409 Conflict`. İkinci `201` cüzdanı iki kez keser. `200` + eski body de vermem: istemci yeni havale sanır.

Debit, credit, `Transfer`, `IdempotencyRecord`, `AuditLog`, `OutboxMessage` **tek SQL commit**. Outbox satırı defterle birlikte yazılır; worker **commit’ten sonra** yayınlar. HTTP timeout olsa niyet veritabanında durur. Hangfire planlanan worker. csproj’da yok.

Kural `src/ClearPay.Domain/Ledger` altında. `LedgerPair` xUnit’te. Compose SQL’e EF bağlamak duruyor. `POST /api/transfers` endpoint değil. Gateway sınıfları `NotImplementedException` atıyor.

## Bugün tıklanan

Cookie Identity, SQLite: `App_Data/identity.db`. Kayıt, giriş, **0,00 ₺**. Özet PageModel’de hâlâ sabit; ledger net değil.

| Sayfa | Rota | Durum |
|-------|------|--------|
| Giriş | `/Account/Login` (`/giris`) | Çalışır |
| Kayıt | `/Account/Register` (`/kayit`) | Çalışır |
| Özet | `/` | Boş özet |
| Havale | `/havale` | Form kabuğu; Gönder kapalı |
| Yükle / Çek | `/yukle-cek` | Form kabuğu; butonlar kapalı |
| Hareketler | `/hareketler` | Boş tablo; filtre kapalı |
| Dekont | — | Yok |
| Admin | — | Yok (menü gizler) |

`GET /api/health` → `{ "status": "ok", "product": "ClearPay" }`. JWT yok, Swagger yok, Hangfire paketi yok, Redis yok, RabbitMQ yok.

`docker compose` SQL Server 2022’yi `localhost,1433`’te açar. Web henüz o veritabanını okumaz. Identity için Docker gerekmez.

## Çalıştırma

.NET 8 SDK. SQL konteyneri için Docker Desktop.

```bash
docker compose up -d
dotnet run --project src/ClearPay.Web --launch-profile http
```

[http://localhost:5153](http://localhost:5153)

```bash
dotnet test
dotnet build ClearPay.slnx
```

Lokal SA şifresi `.env.example` içinde (`ClearPay_Dev1!`). Yalnız Docker. `.env` commit edilmez. Azure’da bu şifre yok.

CI (`main` üzerindeki workflow) `tests/ClearPay.Tests` restore + test eder.

## Düzen

```
src/ClearPay.Domain           LedgerEntry, LedgerPair, Wallet (Balance yok)
src/ClearPay.Application      portlar (ITransferExecutor, IBankGateway, …), FluentValidation
src/ClearPay.Infrastructure   Identity (SQLite), throw eden gateway stub
src/ClearPay.Web              Razor + MapControllers, http profili :5153
tests/ClearPay.Tests          LedgerPair + giriş/sayfa smoke
docker-compose.yml            SQL Server 2022 — web uygulaması değil
ClearPay.slnx
```

Web → Application + Infrastructure. Infrastructure → Application → Domain. Domain EF veya ASP.NET görmez.

## Belgeler

- [`docs/SPEC.md`](docs/SPEC.md) — ekranlar ve para kuralları (409, tek transaction, outbox)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — katmanlar, önce cookie sonra JWT
- [`docs/FARK.md`](docs/FARK.md) — mutabakat; Papara rakibi değil
- [`docs/DEPLOY.md`](docs/DEPLOY.md) — Compose + `dotnet run`

Canlı hedef: Azure App Service + Azure SQL (West Europe). Aboneliği ben açacağım; tıklanacak `azurewebsites.net` yok.

## Lisans

[MIT](LICENSE) © 2026 Halil Mert Develi
