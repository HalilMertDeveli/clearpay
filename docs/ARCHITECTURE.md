# ARCHITECTURE — ClearPay

Kaynak: `SPEC.md`, `PLAN.md`. Kod LED reposuna yazılmaz. Ekran listesi sabittir.

Ürün: WePay benzeri dijital cüzdan **sitesi**. Sahte banka uygulaması değil; sahte olan yalnızca `IBankGateway` (yükle/çek REST+SOAP stub).

## Seçilen mimari (T-019)

Derleme kuralı **Onion / Clean Architecture**’tır. Klasik n-tier’a (UI → BLL → DAL, BLL içinde EF, çift yönlü referans) geçilmez: ledger, idempotency ve 409 iş kuralını PageModel veya “kolay UPDATE Balance” yoluna kilitlemesin diye bağımlılık **içe** bakar.

Aynı dört proje, n-tier bilenlere **isim eşlemesi**dir — ikinci BLL/DAL ağacı yok, ikinci uygulama yok:

| Onion / Clean (csproj kuralı) | n-tier adı (aynı proje) | Hexagonal (aynı soğan) |
|-------------------------------|-------------------------|-------------------------|
| `ClearPay.Domain` (merkez) | iş kuralları / varlık | domain model |
| `ClearPay.Application` | BLL (use case + DTO) | ports (`IWalletReader`, `ITransferExecutor`, `IBankGateway`, …) |
| `ClearPay.Infrastructure` | DAL + dış sistem | adapters (SQL, Identity, Rest/Soap gateway) |
| `ClearPay.Web` | sunum + composition root | host (Razor/HTTP); `new RestBankGateway()` yok |

`csproj` neyi zorunlu kılar: Domain paket/proje referansı yok (EF/HTTP yok). Application yalnız Domain. Infrastructure → Application (+ Domain, EF eşlemesi için). Web → Application + Infrastructure; Web **Domain’e doğrudan ProjectReference vermez**. n-tier cümlesi dokümandadır; derleyici Onion grafiğini tutar.

## Katmanlar (Clean Architecture, tek process)

| Proje | Ne tutar | Ne tutmaz |
|-------|----------|-----------|
| `ClearPay.Domain` | Roller (`Musteri`/`Admin`), para kuralları, `LedgerEntry` anlamı | HTTP, EF, Razor |
| `ClearPay.Application` | Use case, DTO, FluentValidation, port arayüzleri | Connection string, cookie |
| `ClearPay.Infrastructure` | Identity, SQL Server, EF/Dapper, Hangfire, `IBankGateway` | Razor, CSS |
| `ClearPay.Web` | Razor Pages + JSON API, cookie/JWT host | Ledger hesabı, bakiye düzeltme |

Bağımlılık: Web → Application + Infrastructure; Infrastructure → Application → Domain. Domain dışarı bakmaz.

Bugün: Identity + ledger aynı SQL Server `ClearPay` (`AppIdentityDbContext` AspNet* + `ClearPayDbContext` kasa; T-058). Test factory `ClearPay:UseSqliteLedger=true` → SQLite. `SqlOptions` Web `Program.cs`’te değil, `AddClearPay(configuration)` içinde bağlanır. SQL yoksa site ayağa kalkmaz (Identity SQL).

## Ekran haritası (SPEC ↔ Razor)

| # | SPEC | Rota | Durum |
|---|------|------|-------|
| 1 | Giriş | `/Account/Login` | TASK-03 |
| 2 | Kayıt | `/Account/Register` | TASK-03 |
| 3 | Cüzdan özeti | `/` (`Index`) | Boş özet TASK-03; canlı TASK-05 |
| 4 | Havale | `/havale` | TASK-06: form POST → `ITransferExecutor`; `POST /api/transfers` JWT + `Idempotency-Key` (201/409) |
| 5 | Yükle / Çek | `/yukle-cek` | TASK-07 REST; TASK-08 SOAP (`BankGateway:Strategy`). Timeout → ledger yok |
| 6 | Hareketler | `/hareketler` | TASK-09 filtre + sayfa |
| 7 | Dekont | `/dekont/{correlationId}` | TASK-09 correlation id |
| 8 | Admin | `/admin` | TASK-10 rol `Admin`; freeze / kuyruk / audit |

Sol menü SPEC ile aynı: Özet, Havale, Yükle/Çek, Hareketler; Admin yalnızca rol. Logout yardımcı sayfadır, yeni ürün ekranı değil. Satıcı paneli / POS yok (Q2, kapsam dışı).

## Neden tek host

Razor ve JSON API aynı ASP.NET Core 8 uygulamasında. Mülakat omurgası “mikroservis ağı” değil; çift kayıt, 409 ve outbox tek SQL transaction içinde kanıtlanır. Tek deploy: Azure App Service. Kafka / K8s / ayrı Payments servisi Q1 yasak.

## Neden Identity cookie, sonra JWT

- **Site (tarayıcı):** ASP.NET Identity + HttpOnly cookie (`ClearPay.Auth`). Form POST, anti-forgery, sliding expiration. TASK-03.
- **JSON API:** JWT + OpenAPI. `POST /api/transfers` + `Idempotency-Key` (201 / 409). TASK-06 landed. Q2 Flutter: `GET /api/wallet`, movements, receipts; `POST /api/topup` / `withdraw` (T-061). Swagger UI `/swagger` (T-050). Harici client cookie’ye bağlanmaz.
- Aynı kullanıcı deposu; iki protokol. Razor JWT taşımaz; para API’si tarayıcı cookie’sini birincil kimlik saymaz.

## Neden ledger Web’de değil

Para kuralları Domain + Application’dadır (Payments ajanı). Web yalnızca HTTP: sayfa veya 201/409. Çift kayıt, bakiye invarianti, freeze, iade ve outbox insert **aynı SQL transaction**’da Infrastructure’da biter. Ledger PageModel’de olsa 409 ve invariant UI’ye kilitlenir; “UPDATE Balance” yolu açılır — yasak.

Şema (TASK-04 landed): `Wallet` (1 user = 1 wallet, bakiye kolonu yok), `LedgerEntry`, `Transfer`, `IdempotencyRecord` (Key unique), `AuditLog`, `OutboxMessage` (Status; worker TASK-11). İndeks: `LedgerEntry(WalletId, CreatedAt)`. Identity AspNet* aynı SQL Server `ClearPay` (T-058); testler SQLite.

## SOLID haritası

| Sınıf | İlke |
|-------|------|
| `IWalletReader` | ISP + DIP — özet okuma. PageModel ledger net hesaplamaz. TASK-03 `EmptyWalletReader`; TASK-05 SQL. |
| `ITransferExecutor` | SRP + DIP — havale Application port; Web yalnızca HTTP 201/409 (TASK-06). |
| `IIdempotencyStore` | ISP — 409 deposu executor’dan ayrı. |
| `IClock` | ISP — test double; para kuralı değil. |
| `IBankGateway` | OCP + LSP — Rest/Soap aynı sözleşme; Web `switch` yazmaz. |
| `RestBankGateway` / `SoapBankGateway` | OCP strateji (TASK-07/08 stub). |
| `AddClearPay(configuration)` | Composition; `SqlOptions` burada; Web `Application.Ports` enjekte eder. |
| `LedgerPair` (Domain) | SRP para kuralı; Payments. Web’de yok. |

Coder: `Program.cs` içinde `builder.Services.AddClearPay(builder.Configuration);` (`ClearPay.Infrastructure.DependencyInjection`). Havale API bu katmanda açılmaz.

## Q1 vs Q2 — Redis / Rabbit

| | Q1 | Q2 |
|--|----|-----|
| Gerçeklik kaynağı | SQL + outbox satırı | Aynı; broker kopya değil |
| Kuyruk | Hangfire, outbox tablosu (TASK-11) | Rabbit `clearpay.outbox` (T-048; yoksa log). Canlı CloudAMQP Q2 |
| Cache | Redis özet DTO (`localhost:6379`; yoksa SQL) | Aynı; havale sonrası invalidate (TASK-06) |
| Canlı | App Service + Azure SQL | + Azure Cache for Redis |

Compose: SQL gün 1. Redis özet cache T-041 (kasa SQL). Rabbit publisher T-048 (`ConnectionStrings:RabbitMq`; düşerse log). Canlı broker Q1 şart değil — ledger commit olduysa mesaj kaybolmaz (önce DB, sonra yayın). Redis bakiyeyi hızlandırır, ledger’ın yerini tutmaz. Health: `redis` + `rabbit` = `up` / `down` / `off`.
