# ARCHITECTURE — ClearPay

Kaynak: `SPEC.md`, `PLAN.md`. Kod LED reposuna yazılmaz. Ekran listesi sabittir.

## Katmanlar (Clean Architecture, tek process)

| Proje | Ne tutar | Ne tutmaz |
|-------|----------|-----------|
| `ClearPay.Domain` | Roller (`Musteri`/`Admin`), para kuralları, `LedgerEntry` anlamı | HTTP, EF, Razor |
| `ClearPay.Application` | Use case, DTO, FluentValidation, port arayüzleri | Connection string, cookie |
| `ClearPay.Infrastructure` | Identity, SQL Server, EF/Dapper, Hangfire, `IBankGateway` | Razor, CSS |
| `ClearPay.Web` | Razor Pages + JSON API, cookie/JWT host | Ledger hesabı, bakiye düzeltme |

Bağımlılık: Web → Application + Infrastructure; Infrastructure → Application → Domain. Domain dışarı bakmaz.

Bugün (TASK-03): Identity SQLite (`App_Data/identity.db`). Ledger SQL Server TASK-04; uygulama Compose SQL’e o task’ta bağlanır.

## Ekran haritası (SPEC ↔ Razor)

| # | SPEC | Rota | Durum |
|---|------|------|-------|
| 1 | Giriş | `/Account/Login` | TASK-03 |
| 2 | Kayıt | `/Account/Register` | TASK-03 |
| 3 | Cüzdan özeti | `/` (`Index`) | Boş özet TASK-03; canlı TASK-05 |
| 4 | Havale | `/Havale` | Placeholder; API TASK-06 |
| 5 | Yükle / Çek | `/YukleCek` | Placeholder; gateway TASK-07/08 |
| 6 | Hareketler | `/Hareketler` | Placeholder; filtre TASK-09 |
| 7 | Dekont | yok | TASK-09 (`correlation id`) |
| 8 | Admin | yok (menü role gizli) | TASK-10 |

Sol menü SPEC ile aynı: Özet, Havale, Yükle/Çek, Hareketler; Admin yalnızca rol. Logout yardımcı sayfadır, yeni ürün ekranı değil. Satıcı paneli / POS yok (Q2, kapsam dışı).

## Neden tek host

Razor ve JSON API aynı ASP.NET Core 8 uygulamasında. Mülakat omurgası “mikroservis ağı” değil; çift kayıt, 409 ve outbox tek SQL transaction içinde kanıtlanır. Tek deploy: Azure App Service. Kafka / K8s / ayrı Payments servisi Q1 yasak.

## Neden Identity cookie, sonra JWT

- **Site (tarayıcı):** ASP.NET Identity + HttpOnly cookie (`ClearPay.Auth`). Form POST, anti-forgery, sliding expiration. TASK-03.
- **JSON API:** JWT + OpenAPI. `POST /api/transfers` + `Idempotency-Key` (201 / 409). TASK-06. Swagger ve harici client cookie’ye bağlanmaz.
- Aynı kullanıcı deposu; iki protokol. Razor JWT taşımaz; para API’si tarayıcı cookie’sini birincil kimlik saymaz.

## Neden ledger Web’de değil

Para kuralları Domain + Application’dadır (Payments ajanı). Web yalnızca HTTP: sayfa veya 201/409. Çift kayıt, bakiye invarianti, freeze, iade ve outbox insert **aynı SQL transaction**’da Infrastructure’da biter. Ledger PageModel’de olsa 409 ve invariant UI’ye kilitlenir; “UPDATE Balance” yolu açılır — yasak.

Şema (TASK-04 Domain POCOs var; EF Coder): `Wallet` (1 user = 1 wallet, bakiye kolonu yok), `LedgerEntry`, `Transfer`, `IdempotencyRecord` (Key unique), `AuditLog`, `OutboxMessage`. İndeks: `LedgerEntry(WalletId, CreatedAt)`. Identity SQLite ayrı; ledger SQL Server.

## SOLID haritası

| Sınıf | İlke |
|-------|------|
| `IWalletReader` | ISP + DIP — özet okuma. PageModel ledger net hesaplamaz (TASK-05). |
| `ITransferExecutor` | SRP + DIP — havale Application port; Web yalnızca HTTP 201/409 (TASK-06). |
| `IIdempotencyStore` | ISP — 409 deposu executor’dan ayrı. |
| `IClock` | ISP — test double; para kuralı değil. |
| `IBankGateway` | OCP + LSP — Rest/Soap aynı sözleşme; Web `switch` yazmaz. |
| `RestBankGateway` / `SoapBankGateway` | OCP strateji (TASK-07/08 stub). |
| `AddClearPay()` | Composition; Web `Application.Ports` enjekte eder. |
| `LedgerPair` (Domain) | SRP para kuralı; Payments. Web’de yok. |

Coder: `Program.cs` içinde `builder.Services.AddClearPay();` (`ClearPay.Infrastructure.DependencyInjection`). Havale API bu katmanda açılmaz.

## Q1 vs Q2 — Redis / Rabbit

| | Q1 | Q2 |
|--|----|-----|
| Gerçeklik kaynağı | SQL + outbox satırı | Aynı; broker kopya değil |
| Kuyruk | Hangfire, outbox tablosu (TASK-11) | Rabbit publisher (TASK-12 Compose; canlı CloudAMQP) |
| Cache | Yok / sonra | Redis özet bakiyesi; havale sonrası invalidate |
| Canlı | App Service + Azure SQL | + Azure Cache for Redis |

Compose: SQL gün 1; Redis/Rabbit TASK-12 lokal. Canlı broker Q1 şart değil — ledger commit olduysa mesaj kaybolmaz (önce DB, sonra yayın). Redis bakiyeyi hızlandırır, ledger’ın yerini tutmaz.
