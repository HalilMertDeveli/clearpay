# ClearPay

| **English** | [Türkçe](./README.tr.md) | [Deutsch](./README.de.md) | [Français](./README.fr.md) |
|:-----------:|:-----------------------:|:------------------------:|:--------------------------:|

<p align="center">

<strong>English</strong> · [Türkçe](./README.tr.md) · [Deutsch](./README.de.md) · [Français](./README.fr.md)

</p>

<p align="center">
  <img src="docs/assets/clearpay-mark.png" width="96" alt="ClearPay mark">
</p>

<p align="center">
  <a href="https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml"><img src="https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml/badge.svg" alt="CI"></a>
  <img src="https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/Flutter-JWT_client-02569B?logo=flutter" alt="Flutter JWT client">
  <img src="https://img.shields.io/badge/SQL_Server-ledger-CC2927?logo=microsoftsqlserver" alt="SQL Server ledger">
  <img src="https://img.shields.io/badge/UI-TR%20%7C%20EN%20%7C%20DE%20%7C%20FR-1B2A4A" alt="UI languages">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT"></a>
</p>

<p align="center">
  <img src="docs/assets/clearpay-hero.png" alt="ClearPay — demo digital wallet, ASP.NET Core 8, Flutter, one SQL ledger. No UPDATE Balance." width="920">
</p>

<p align="center">
  <b>Demo digital wallet</b> — ASP.NET Core 8 + Flutter, one SQL Server ledger, no <code>UPDATE Balance</code>.<br>
  Fake bank gateway. <b>Not</b> a licensed e-money institution. <b>Not</b> Papara / FAST / a retail bank clone.
</p>

I am **Halil Mert Develi**. This is the interview repo I defend (Intertech, Softtech): double-entry, idempotent HTTP, two clients, one cash register.

<p align="center">
  <img src="docs/assets/clearpay-rules.png" alt="Balance is derived; replay is 409; one SQL transaction" width="920">
</p>

**Backend deep dives:** [Card payments (mock 3D Secure)](#card-payments-mock-3d-secure) · [SQL performance (measured)](#sql-performance-measured) · [Tracing a payment](#tracing-a-payment-observability) · [Services and messaging](#services-and-messaging-rabbitmq) · [Docs](#docs)

---

## Website

Razor Pages at [http://localhost:5153](http://localhost:5153) (Development seed `admin@clearpay.test` / `Deneme123`). A Canada Central App Service hostname exists, but `/api/health` still returns **404** — live HTTPS is **TASK-16**, so these shots are **local**. Not a licensed bank UI.

| Sign-in `/giris` | Summary after login |
|:----------------:|:-------------------:|
| <img src="docs/assets/shot-giris.png" alt="ClearPay website sign-in" width="420"> | <img src="docs/assets/shot-ozet.png" alt="ClearPay website summary" width="420"> |
| Language bar TR · EN · DE · FR. Demo wallet. | Same SQL ledger. Balance is `LedgerPair.NetOf`. |

| Register `/kayit` | Cards `/kartlar` |
|:-----------------:|:----------------:|
| <img src="docs/assets/shot-kayit.png" alt="ClearPay website register" width="420"> | <img src="docs/assets/shot-kartlar.png" alt="ClearPay website cards" width="420"> |
| Cookie Identity. Same four languages. | Last four + scheme only. No PAN in SQL. Fake gateway. |

---

## Mobile app

Flutter JWT client on Android emulator `emulator-5554` → `http://10.0.2.2:5153`. Same wallet screens as the website (including Kartlarım), same SQL. **Not** a Hive / Firestore cash register. Firestore may write `app_meta/ping` only.

<p align="center">
  <img src="docs/assets/shot-mobile.png" alt="ClearPay Flutter overview on Android emulator" width="280">
</p>

<p align="center"><i>Özet — language strip in chrome, demo footer. Wallet rows come from JWT → SQL (spinner while the local API answers).</i></p>

<p align="center">
  <img src="docs/assets/clearpay-clients.png" alt="Website Razor cookie vs Flutter JWT — one SQL ledger" width="840">
</p>

---

## Why it exists

Most demo wallets store a number on `Wallet.Balance` and patch it. ClearPay does not.

| Rule | What the code does |
|------|-------------------|
| **Balance is derived** | `LedgerPair.NetOf` over signed `LedgerEntry` rows. There is no balance column. |
| **Replay is 409** | Same `Idempotency-Key` = same intent. A timeout retry must not debit twice. |
| **One SQL transaction** | Debit, credit, transfer, idempotency, audit, and outbox commit together. |
| **Outbox first** | The message row is in that transaction. After commit a Hangfire job publishes it to RabbitMQ and marks it Sent only on a publisher confirm. |
| **Two clients, one ledger** | Website (cookie) and Flutter (JWT) hit the same Application ports. The phone has no Hive / SQLite / Firestore wallet. |

---

## Two clients

| | Website | Mobile app |
|--|---------|------------|
| Path | `src/ClearPay.Web` | [`mobile/clearpay`](mobile/clearpay) |
| UI | Razor Pages, TR / EN / DE / FR | Flutter 3.41, TR default, same four languages |
| Auth | ASP.NET Identity cookie | JWT Bearer (`POST /api/token`) |
| Money | Application ports → SQL | **Same ports.** No second cash register |
| Extra | [`/kartlar`](http://localhost:5153/kartlar) — demo linked cards (last four + scheme, no PAN) | Flutter **Kartlarım** — same last four + scheme; `GET/POST /api/cards`; PAN not in SQL |

Flutter **web is not a product**. The browser product is Razor. Android emulator uses `http://10.0.2.2:5153`; Windows / iOS use `http://localhost:5153`.

---

## Run it

.NET 8 SDK. Development uses **SQL Server LocalDB** `(localdb)\MSSQLLocalDB` / database `ClearPay` for Identity **and** the ledger.

```bash
dotnet run --project src/ClearPay.Web --launch-profile http
```

Open [http://localhost:5153/giris](http://localhost:5153/giris) — `admin@clearpay.test` / `Deneme123` (Development seed only).

```bat
cd /d D:\ClearPay\clearpay\mobile\clearpay
flutter run -d emulator-5554
```

```bash
dotnet test
dotnet build ClearPay.slnx
```

OpenAPI: [http://localhost:5153/swagger](http://localhost:5153/swagger) · health: [http://localhost:5153/api/health](http://localhost:5153/api/health)

Docker Desktop is optional (SQL 2022 + Redis + Rabbit). App money is **SQL Server only**. MySQL on this machine is a sidecar / Workbench tool — not the wallet. Do not commit `.env`. A Canada Central hostname exists; `/api/health` is still **404**. Live hosting is TASK-16 (`docs/CANLI.md`). This README does not treat that URL as a working product.

---

## What you can click

| | Website | Flutter |
|--|---------|---------|
| Sign in | [`/giris`](http://localhost:5153/giris) | Giriş — email or demo TC `10000000146` |
| Register | [`/kayit`](http://localhost:5153/kayit) | Hesap oluştur |
| Summary | [`/`](http://localhost:5153/) | Özet — `GET /api/wallet` |
| Transfer | [`/havale`](http://localhost:5153/havale) | Havale — `POST /api/transfers` + `Idempotency-Key` → **201 / 409** |
| Cards | [`/kartlar`](http://localhost:5153/kartlar) | Parked (same SQL instruments via API) |
| Top-up / withdraw | [`/yukle-cek`](http://localhost:5153/yukle-cek) | Yükle / Çek — fake gateway, including `TIMEOUT` |
| Movements | [`/hareketler`](http://localhost:5153/hareketler) | Hareketler |
| Receipt | [`/dekont/{id}`](http://localhost:5153/hareketler) | Dekont + PDF bytes from SQL |
| Admin | [`/admin`](http://localhost:5153/admin) | Admin tab (role) |

Language pickers are chrome (cookie `c=` on the site; local file on Flutter), not a tenth screen.

---

## Picture of the build

<p align="center">
  <img src="docs/assets/clearpay-layers.png" alt="ClearPay Clean Architecture layers" width="840">
</p>

Web never computes ledger math. The summary page asks `IWalletReader`. Today that adapter is `SqlWalletReader`: balance = `LedgerPair.NetOf`, this month in/out, last five rows, freeze badge. If SQL is down you still get the site — zeros, not a 500.

<p align="center">
  <img src="docs/assets/clearpay-ledger.png" alt="ClearPay double-entry pair" width="840">
</p>

```mermaid
flowchart TB
  subgraph clients [Same person]
    razor[Website Razor cookie]
    flutter[Flutter app JWT]
  end
  subgraph web [ClearPay.Web]
    pages[Razor Pages TR/EN/DE/FR]
    api[JSON API]
  end
  subgraph app [ClearPay.Application]
    reader[IWalletReader]
    exec[ITransferExecutor]
  end
  subgraph infra [ClearPay.Infrastructure]
    sql[SqlWalletReader + EF]
    id[Identity]
  end
  subgraph domain [ClearPay.Domain]
    pair[LedgerPair / LedgerEntry]
  end
  razor --> pages
  flutter --> api
  pages --> reader
  pages --> exec
  api --> reader
  api --> exec
  reader --> sql
  exec --> pair
  sql --> pair
```

| Layer | Project | Holds | Must not hold |
|-------|---------|-------|----------------|
| UI + host | `ClearPay.Web` | Razor, cookie, culture cookie, `:5153` | Ledger net, `UPDATE Balance` |
| Use cases | `ClearPay.Application` | Ports, DTOs, FluentValidation | Connection strings |
| Adapters | `ClearPay.Infrastructure` | EF SQL Server (Identity + ledger), gateway stubs | Razor / CSS |
| Rules | `ClearPay.Domain` | `LedgerPair`, `Wallet` (no balance field) | EF, HTTP, ASP.NET |

Dependencies point **inward**. Domain does not reference EF or ASP.NET.

---

## Relational schema (SQL Server)

**Demo — fake bank gateway.** Not licensed e-money. Identity and the ledger share one LocalDB database (two EF contexts, two history tables). `Wallet.UserId` matches `AspNetUsers.Id` with **no FK** (two DbContexts). Real FKs are Identity membership plus `LedgerEntry` → `Wallet` / `Transfer`.

`LinkedInstrument` stores **last four + scheme + label** only. PAN / CVV are never persisted.

```mermaid
erDiagram
    AspNetUsers {
        string Id PK
        string FullName
        string Email
        string AccountKind
        string UserName
    }
    AspNetRoles {
        string Id PK
        string Name
    }
    AspNetUserRoles {
        string UserId PK
        string RoleId PK
    }
    Wallet {
        uniqueidentifier Id PK
        string UserId UK
        bit IsFrozen
        datetimeoffset CreatedAt
    }
    LedgerEntry {
        uniqueidentifier Id PK
        uniqueidentifier WalletId FK
        decimal Amount
        uniqueidentifier PairId
        uniqueidentifier CorrelationId
        uniqueidentifier TransferId FK
        int Kind
        nvarchar Description
        datetimeoffset CreatedAt
    }
    Transfer {
        uniqueidentifier Id PK
        uniqueidentifier FromWalletId FK
        uniqueidentifier ToWalletId FK
        decimal Amount
        int Status
        uniqueidentifier CorrelationId
        datetimeoffset CreatedAt
    }
    IdempotencyRecord {
        nvarchar Key PK
        nvarchar Scope
        nvarchar RequestHash
        uniqueidentifier ResourceId
        datetimeoffset CreatedAt
    }
    AuditLog {
        uniqueidentifier Id PK
        string ActorUserId
        nvarchar Action
        uniqueidentifier CorrelationId
        nvarchar Details
        datetimeoffset CreatedAt
    }
    OutboxMessage {
        uniqueidentifier Id PK
        nvarchar Type
        nvarchar Payload
        uniqueidentifier CorrelationId
        int Status
        datetimeoffset OccurredAt
        datetimeoffset ProcessedAt
    }
    LinkedInstrument {
        uniqueidentifier Id PK
        string UserId
        nvarchar Last4
        nvarchar Label
        nvarchar Scheme
        datetimeoffset CreatedAt
    }
    AspNetUsers ||--o{ AspNetUserRoles : UserId
    AspNetRoles ||--o{ AspNetUserRoles : RoleId
    Wallet ||--o{ LedgerEntry : WalletId
    Wallet ||--o{ Transfer : FromWalletId
    Wallet ||--o{ Transfer : ToWalletId
    Transfer |o--o{ LedgerEntry : TransferId
```

`IdempotencyRecord.Key` is unique (replay → **409**). History tables: `__EFMigrationsHistory` (ledger) and `__EFMigrationsHistoryIdentity`.

---

## Firebase is not the cash register

Flutter initializes Firebase project `clearpay-c0485`. It may write **`app_meta/ping`** (`ok`, `client`, `message`, `touchedAt`) so the console can prove the client is alive.

Money does **not** go there. Balance, transfers, and receipts stay JWT → ASP.NET → SQL Server. Firestore rules deny every other path. Windows desktop skips native Firebase plugins; use the **Android emulator** to see the ping on the sign-in screen.

---

## Card payments (mock 3D Secure)

**Mock card acquiring, not a real bank.** A merchant starts a card payment, the shopper goes through a fake 3D Secure page, and the result comes back signed. The three banks (`MockBankA`, `MockBankB`, `MockPspC`), the ACS page and the SMS code are stubs that run in the same host. No real card is charged. The PAN is used only for BIN routing and the last four digits, then it is dropped.

```mermaid
sequenceDiagram
    autonumber
    participant S as Shopper browser
    participant M as Merchant
    participant G as ClearPay gateway
    participant A as Mock ACS
    participant B as Mock bank / PSP adapter
    M->>G: POST /api/payments (X-Merchant-Key)
    G-->>M: 201 Pending3DS + redirectUrl
    M-->>S: redirect to /mock-acs/{id}
    S->>A: OTP (123456 = success)
    A-->>S: auto-post {result, HMAC-SHA256}
    S->>G: POST /api/payments/3ds/callback
    G->>G: verify HMAC with stored amount + 3DS id
    G->>B: authorize (only if 3DS = Y)
    B-->>G: raw code, normalized
    G-->>S: 302 merchant returnUrl (Authorized / Failed)
    M->>G: capture · void · refunds
```

| Rule | What the code does |
|------|-------------------|
| **Explicit state machine** | `Created → Pending3DS → Authorized/Failed → Captured → PartiallyRefunded/Refunded`, plus `Authorized → Voided`. Any other move is a domain error (409). |
| **Signed callback** | HMAC over `paymentId`, 3DS id, result and amount, compared in constant time. A tampered result gets 400 and the payment stays `Pending3DS`. |
| **Idempotent callback** | A second callback does nothing and returns the same redirect: no second bank call, no second event. Parallel duplicates lose on the `Version` concurrency token. |
| **One contract, three banks** | ISO 8583 codes, XML and JSON answers are normalized behind `IAcquirerAdapter`. A BIN router picks the adapter. |
| **Refund limit under concurrency** | The refund amount is reserved and committed first, then the bank is called, then the refund is settled. Total refunds never exceed the captured amount. |
| **Outbox** | Every state change writes a `clearpay.payment.*` row into `OutboxMessage` in the same `SaveChanges`. |

Click through it at [`/demo-magaza`](http://localhost:5153/demo-magaza) (Development; merchant key `dev-demo-shop-key`). Test cards, trigger amounts and known limits are in [`docs/PAYMENTS.md`](docs/PAYMENTS.md).

---

## SQL performance (measured)

The summary and statement screens used to load **every ledger row of the wallet** into C#. They now call T-SQL stored procedures (`usp_Wallet_GetSummary`, `usp_Wallet_GetStatementPage`; `usp_Ledger_GetDailyTotals` is ready for reconciliation) backed by a covering `(WalletId, CreatedAt)` index plus `PairId` and `CreatedAt` indexes — one EF migration, `CREATE OR ALTER` scripts in [`Persistence/Sql`](src/ClearPay.Infrastructure/Persistence/Sql).

Measured on a throwaway SQL Server 2022 container, 600,000 ledger rows, hot treasury wallet = 120,000 rows (warm cache, laptop):

| Query (treasury wallet) | Logical reads before → after | Elapsed before → after | Plan before → after |
|-------------------------|-----------------------------:|-----------------------:|---------------------|
| Summary (balance + month in/out) | 368,223 → **1,392** | 291 ms → **30 ms** | Seek + 120k Key Lookups → covering Seek |
| Statement page 1 | 127,269 → **1,495** | 319 ms → **15 ms** | Clustered Scan + per-row counterparty scans → Seeks |
| Daily totals (1 day) | 10,783 → **13** | 10 ms → **0 ms** | Clustered Scan → Seek |

Full before / after table, method, costs (index space, write probe inconclusive) and the actual `.sqlplan` files: [`docs/SQL-PERF.md`](docs/SQL-PERF.md). Reproduce with `scripts/perf/run-perf.ps1`. Demo data, not a production benchmark.

---

## Tracing a payment (observability)

Every response carries `X-Trace-Id` and `X-Correlation-Id`. Paste either into Seq or Grafana/Tempo and you get the request log line, the `payment topup` / `payment transfer` span, and the SQL spans under it — including the single `INSERT [AuditLog] [IdempotencyRecord] [LedgerEntry] [OutboxMessage]` batch. A mock 3D Secure card payment shows up as `payment card_create` → `payment card_3ds` → child `acquirer authorize` (bank name, normalized and raw response code) → `payment card_capture` / `card_refund`; a forged 3DS signature or an unavailable issuer marks the span as an error.

```bash
docker compose -f docker-compose.observability.yml -p clearpay-obs up -d   # Seq, OTel Collector, Tempo, Prometheus, Grafana
dotnet run --project src/ClearPay.Web --launch-profile http                # Development exports to :5341 / :4317
```

| | |
|--|--|
| Logs | Serilog → console + Seq ([localhost:8341](http://localhost:8341)), enriched with `TraceId`, `SpanId`, `CorrelationId`, opaque `UserId`. PAN-looking numbers and secret-named properties are masked. |
| Traces | OpenTelemetry: ASP.NET Core, HttpClient, SqlClient, `ClearPay.Payments` (wallet + card payments, acquirer calls), `ClearPay.Messaging` → Collector → Tempo + Seq. No PAN, BIN or amount on a span. |
| Metrics | `clearpay_payments_total`, `clearpay_payments_duration_milliseconds`, HTTP RED → Prometheus → Grafana dashboard *ClearPay - Payments* ([localhost:3000](http://localhost:3000)) |

**Honest scope:** the stack runs in local Docker only (no auth, dev settings); Production config leaves `Observability` empty. The trace now crosses RabbitMQ: the outbox row stores the request's `traceparent`, the publisher puts it in a message header and the consumer span in the notification / reconciliation service continues the same trace (see [Services and messaging](#services-and-messaging-rabbitmq)). Runbook: [`docs/OBSERVABILITY.md`](docs/OBSERVABILITY.md).

---

## Services and messaging (RabbitMQ)

Notifications and nightly reconciliation run as **two separate processes** that only talk to the web app through RabbitMQ events. The web app stays one host for money (ledger, card payments, outbox).

```mermaid
flowchart LR
  subgraph web [ClearPay.Web - one host]
    api[Merchant API + mock 3DS]
    db[(SQL: ClearPay<br/>Payment + OutboxMessage<br/>same transaction)]
    relay[Hangfire outbox relay<br/>publisher confirms]
    api --> db --> relay
  end
  relay -->|"persistent, MessageId = outbox id, traceparent header"| ex{{"clearpay.events topic exchange"}}
  ex -->|"payment.* and reconciliation.mismatch_found"| nq[(notifications.events)]
  ex -->|"payment.captured, payment.refunded"| rq[(reconciliation.payments)]
  nq -.->|nack| nr[(".retry, TTL 5 s")]
  nr -.->|expired| nq
  nq -.->|"3rd failure or poison"| nd[(".dlq")]
  rq -.->|"same retry and DLQ"| rq
  nq --> notif[ClearPay.Notifications worker<br/>fake e-mail / SMS / webhook]
  notif --> ndb[(SQL: ClearPayNotifications<br/>notif.NotificationLog<br/>notif.ProcessedMessage)]
  rq --> recon[ClearPay.Reconciliation<br/>Hangfire 02:00 Europe/Istanbul]
  recon --> rdb[(SQL: ClearPayReconciliation<br/>recon.PaymentTransaction<br/>recon.ReconciliationReport)]
  files[/mock bank settlement CSVs/] --> recon
  recon -.->|"EXEC usp_Ledger_GetDailyTotals, read-only"| db
  recon -->|mismatch_found| ex
```

| What | How |
|------|-----|
| No lost events | Outbox row in the payment's transaction; relay marks it Sent only after a **publisher confirm**. Broker down → row stays Pending with `Attempts` / `LastError` (failed after 10 tries). |
| Delayed retry | Consumer nacks → work queue dead-letters into a `.retry` queue with a 5 s TTL → back to the work queue. Attempt number comes from RabbitMQ's `x-death` count. |
| Dead-letter queue | After 3 attempts, or at once for poison (bad JSON / no MessageId), the message is copied to `.dlq` with `x-clearpay-error` and acked. |
| Idempotent consumers | `ProcessedMessage` table per service, unique `(Consumer, MessageId)`; handler rows and that row commit in **one transaction**. |
| Nightly reconciliation | Projection of captured / refunded events vs. each acquirer's settlement CSV → `Matched` / `OnlyAtBank` / `OnlyInClearPay` / `AmountMismatch` + wallet-ledger pair check through the T-SQL team's stored procedure → report + `mismatch_found` event → notifications. |
| Shared contracts | `ClearPay.Contracts` (event types, `PaymentEventV1`) has no dependencies; `ClearPay.Messaging` holds the RabbitMQ plumbing both sides use. |

```bash
docker compose -f docker-compose.services.yml -p clearpay-jobprep up -d --build    # SQL 2022, RabbitMQ, web, notifications, reconciliation
./scripts/demo-card-payment.ps1 -BaseUrl http://localhost:18080 -RefundAmount 20.10   # create -> mock ACS -> OTP -> capture -> refund
./scripts/demo-card-payment.ps1 -BaseUrl http://localhost:18080 -OrderId fail-001    # injected consumer fault -> retry x3 -> DLQ
curl -X POST "http://localhost:18081/api/reconciliation/runs?date=2026-11-09"       # run reconciliation now; Hangfire UI on :18081/hangfire
docker compose -f docker-compose.services.yml -p clearpay-jobprep down -v
```

Verified on that stack (details in the docs): the capture request's `TraceId` shows up unchanged in the notification consumer's log; the `fail-` order went attempt 1 → 2 → 3 → `notifications.events.dlq`; with RabbitMQ stopped a payment still succeeded and its outbox rows waited as Pending, then drained when the broker came back; the reconciliation run produced one of each outcome and the ops e-mail. Broker tests (retry → DLQ, poison, duplicate MessageId, trace parent) also run in CI against a RabbitMQ service container.

**Honest scope:** banks, settlement files, e-mail, SMS and webhooks are **mock** — the settlement CSV is generated from ClearPay's own projection with documented anomalies (amount ending .37 / .73 / .66), because the mock acquirers keep no state. One SQL Server instance with **one database per service** in the compose demo; the only cross-database touch is the read-only stored procedure call. Single RabbitMQ node, fixed 5 s retry delay, no auth on the reconciliation API (internal network only). Details: [`docs/MESSAGING.md`](docs/MESSAGING.md), [`docs/RECONCILIATION.md`](docs/RECONCILIATION.md).

---

## Interview (three sentences)

1. Same `Idempotency-Key` is the same intent: the second HTTP is **409 Conflict** so a timeout retry does not debit twice.
2. Debit, credit, transfer row, idempotency, audit, and outbox commit in **one SQL transaction**; there is no `UPDATE Balance` — balance is `LedgerPair.NetOf`.
3. The outbox row is written in that same transaction so a timeout cannot lose the message; after commit it goes to RabbitMQ with a publisher confirm, and consumers drop duplicates by MessageId.

---

## CV bullets (intended)

Copy these into LinkedIn / a résumé. Do not add Papara, FAST, licensed e-money, or “I shipped a payments company.”

- Built **ClearPay**, an ASP.NET Core 8 **wallet demo** with idempotent P2P transfers, JWT/cookie auth, and a double-entry ledger on SQL Server (`LedgerPair.NetOf`; no `UPDATE Balance`).
- Same `Idempotency-Key` returns **409 Conflict**; ledger + outbox commit in **one SQL transaction**. Mock BankGateway over REST and SOAP. Razor Pages + Flutter JWT share that ledger. SignalR refreshes the other client (not a second cash register).
- Shipped Docker Compose, xUnit tests, Serilog correlation, and GitHub Actions CI. Public Azure HTTPS is **TASK-16** (you add the publish-profile secret) — not a licensed e-money product.

Full CV pack (TR/EN HTML): `C:\Users\clt\Desktop\Halil_Mert_Develi_CV_Paket`. Repo copy: [`docs/CV-HALIL.md`](docs/CV-HALIL.md).

---

## Repo map

```
src/ClearPay.Domain           LedgerEntry, LedgerPair, Wallet (no Balance), Payments/ (card payment state machine)
src/ClearPay.Application      IWalletReader, ITransferExecutor, IBankGateway, IPaymentGateway, IAcquirerAdapter
src/ClearPay.Infrastructure   SqlWalletReader, EF SQL Server, Identity, Payments/ (mock banks), Persistence/Sql (stored procedures), Observability/
src/ClearPay.Web              Razor + localization + MapControllers, Observability/ (Serilog + OpenTelemetry), Pages/Payments (mock ACS, demo shop)
src/ClearPay.Contracts        Event types and payloads shared by producer and consumers (no dependencies)
src/ClearPay.Messaging        RabbitMQ topology, confirming publisher, consumer host (retry / DLQ), ProcessedMessage idempotency, trace headers
src/ClearPay.Notifications    Worker service: payment + reconciliation events -> fake e-mail / SMS / webhook (notif schema)
src/ClearPay.Reconciliation   Nightly settlement reconciliation: Hangfire job + dashboard, projection consumer, API (recon schema)
mobile/clearpay               Flutter JWT client (not in the .slnx)
tests/ClearPay.Tests          LedgerPair, 409, architecture, culture, payments, stored procedures, tracing, outbox
tests/ClearPay.Services.Tests Notifications, reconciliation, retry policy; RabbitMQ integration tests (CLEARPAY_TEST_RABBITMQ)
scripts/perf/                 Seed + measurement scripts for docs/SQL-PERF.md (throwaway SQL Server)
scripts/demo-card-payment.ps1 One mock 3DS payment end to end (create, ACS, OTP, capture, refund)
infra/                        Bicep unused on the live site (T-104); you click Azure Portal
docker-compose.yml            SQL Server 2022 — not the web app
docker-compose.services.yml   Web + notifications + reconciliation + RabbitMQ + throwaway SQL (demo stack)
docker-compose.observability.yml  Seq + OTel Collector + Tempo + Prometheus + Grafana (local)
infra/observability/          Collector, Tempo, Prometheus, Grafana provisioning + dashboard
ClearPay.slnx
```

---

## Roadmap (honest)

| Done | Next |
|------|------|
| TASK-01…15 — screens, ledger, 409, gateway, outbox, Redis/Rabbit, tests, Swagger | **TASK-16** — App Service `ClearPay` exists; `/api/health` is still **404**. You add GitHub secret `AZURE_WEBAPP_PUBLISH_PROFILE` + Portal startup `dotnet ClearPay.Web.dll`. Do **not** run `.\infra\deploy.ps1` (it would replace the live site). |
| Flutter JWT app, Kartlarım (web + Flutter), Firestore ping (meta only) | Store listing / working public HTTPS still TASK-16 |
| Mock 3D Secure card payments (3 mock banks, refund / partial refund / void), ledger stored procedures with measured plans, Serilog + OpenTelemetry tracing | Run the services stack with the observability stack and capture one trace screenshot for this README |
| RabbitMQ with publisher confirms, TTL retry queue + dead-letter queue, idempotent consumers; notifications and nightly reconciliation (Hangfire, Europe/Istanbul) as separate services | Exponential retry tiers, a DLQ re-drive tool, an ops screen to resolve reconciliation differences; real SFTP settlement files are out of scope |

CI on `main` runs `tests/ClearPay.Tests` (with a throwaway SQL Server) and `tests/ClearPay.Services.Tests` (with a throwaway RabbitMQ), and builds the two service images.

---

## Docs

- [`docs/SPEC.md`](docs/SPEC.md) — screens and money rules
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — onion layers
- [`docs/PAYMENTS.md`](docs/PAYMENTS.md) — mock card payments: 3D Secure, acquirer adapters, refunds
- [`docs/SQL-PERF.md`](docs/SQL-PERF.md) — stored procedures, indexes, measured before / after with execution plans
- [`docs/OBSERVABILITY.md`](docs/OBSERVABILITY.md) — find a payment by TraceId; error-analysis runbook
- [`docs/MESSAGING.md`](docs/MESSAGING.md) — RabbitMQ topology, outbox publishing, retry / DLQ, idempotent consumers, live run
- [`docs/RECONCILIATION.md`](docs/RECONCILIATION.md) — nightly settlement reconciliation, mock bank files, ledger check, live run
- [`docs/CANLI.md`](docs/CANLI.md) — Azure click list (no secrets in git)
- [`docs/YOL.md`](docs/YOL.md) — career path; live URL is TASK-16
- [`docs/FARK.md`](docs/FARK.md) — reconciliation-first, not a Papara rival
- [`mobile/clearpay/README.md`](mobile/clearpay/README.md) — Flutter client
- [`docs/API-ESZAMAN.md`](docs/API-ESZAMAN.md) — SignalR hub (not the ledger)

## License

[MIT](LICENSE) © 2026 Halil Mert Develi
