# ClearPay

<p align="center">
  <a href="README.md">English</a>
  · <a href="README.tr.md">Türkçe</a>
  · <b>Deutsch</b>
  · <a href="README.fr.md">Français</a>
</p>

<p align="center">
  <a href="https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml"><img src="https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml/badge.svg" alt="CI"></a>
  <img src="https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/Flutter-mobile_app-02569B?logo=flutter" alt="Flutter-Mobil-App">
  <img src="https://img.shields.io/badge/Android%20%7C%20Windows%20%7C%20iOS-geliefert-0F766E" alt="Android Windows iOS">
  <img src="https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver" alt="SQL Server">
  <img src="https://img.shields.io/badge/UI-TR%20%7C%20EN%20%7C%20DE%20%7C%20FR-1B2A4A" alt="UI-Sprachen">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT"></a>
</p>

<p align="center"><b>Demo — gefälschtes Gateway für Aufladungen.</b> Kein lizenziertes E-Geld-Institut. Nicht Papara / FAST / keine gefälschte Filialbank.</p>

## Web + Mobil (geliefert)

Dieses Repo ist **keine reine Website**. Die **Flutter-App** liegt in [`mobile/clearpay`](mobile/clearpay) und spricht denselben ASP.NET-Core-8-Host. Acht Operationen, ein SQL-Ledger, **kein** zweiter Saldo auf dem Telefon. Details: [`mobile/clearpay/README.md`](mobile/clearpay/README.md).

**Ein Wallet, zwei Clients.** Dieselbe Person meldet sich an, überweist, lädt auf und öffnet den Beleg **auf der Website** und **in der Flutter-App**. Razor Pages (Cookie); JSON (JWT). Doppelte Buchführung im Domain — `Wallet` hat **keine** Spalte `Balance`.

Ich bin Halil Mert Develi. Das ist das .NET-Interview-Repo, das ich verteidigen will (Intertech, Softtech). Lizenz MIT.

---

## Was gebaut ist

![Clean-Architecture-Schichten](docs/assets/clearpay-layers.svg)

Das Web rechnet kein Ledger. Die Übersicht fragt `IWalletReader`. Heute ist der Adapter `SqlWalletReader`: Saldo = `LedgerPair.NetOf`, Monat ein/aus, letzte fünf Zeilen, Freeze-Badge. Ist SQL Server down, läuft die Site trotzdem — Nullen, kein 500.

![Buchungspaar](docs/assets/clearpay-ledger.svg)

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

| Schicht | Projekt | Enthält | Enthält nicht |
|---------|---------|---------|----------------|
| UI + Host | `ClearPay.Web` | Razor, Cookie, Culture-Cookie, `:5153` | Ledger-Netto, `UPDATE Balance` |
| Use Cases | `ClearPay.Application` | Ports, DTOs, FluentValidation | Connection Strings |
| Adapter | `ClearPay.Infrastructure` | EF SQL Server (Identity + Ledger, dieselbe LocalDB), Gateway-Stubs | Razor / CSS |
| Regeln | `ClearPay.Domain` | `LedgerPair`, `Wallet` (kein Saldo-Feld) | EF, HTTP, ASP.NET |

Abhängigkeiten zeigen **nach innen**. Domain kennt weder EF noch ASP.NET.

---

## Relationales Schema (SQL Server)

**Demo — sahte banka gateway. Lisanslı e-para değil.** Nicht Papara / FAST / keine gefälschte Filialbank. Acht Bildschirme, kein neunter.

Local Development: `(localdb)\MSSQLLocalDB` / Datenbank `ClearPay`. Identity und Ledger teilen diese eine Datenbank (zwei EF-Kontexte, zwei History-Tabellen). **Zwei Clients, ein SQL-Ledger:** Razor (Cookie) und Flutter (JWT). Flutter `firebase_core` Projekt `clearpay-c0485` — kein Firestore-Wallet. MySQL (`ConnectionStrings:MySql`) ist Beifahrer; Geld liegt nicht dort.

Es gibt **keine** Spalte `Wallet.Balance`. Saldo = `LedgerPair.NetOf` (C#, keine Tabelle). `UPDATE Balance` ist verboten. `Wallet.UserId` ist unique und entspricht `AspNetUsers.Id` in derselben DB; **kein FK** (zwei DbContexts). Echte FKs: Identity plus `LedgerEntry` → `Wallet` / `Transfer` und `Transfer` → `Wallet`.

Dasselbe Mermaid steht in der GitHub-Standard-README (`README.md`, Abschnitt **Relational schema (SQL Server)**).

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
    AspNetUserClaims {
        int Id PK
        string UserId FK
        string ClaimType
        string ClaimValue
    }
    AspNetRoleClaims {
        int Id PK
        string RoleId FK
        string ClaimType
        string ClaimValue
    }
    AspNetUserLogins {
        string LoginProvider PK
        string ProviderKey PK
        string UserId FK
    }
    AspNetUserTokens {
        string UserId PK
        string LoginProvider PK
        string Name PK
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
        datetimeoffset CreatedAt
    }
    EFMigrationsHistory {
        nvarchar MigrationId PK
        nvarchar ProductVersion
    }
    EFMigrationsHistoryIdentity {
        nvarchar MigrationId PK
        nvarchar ProductVersion
    }
    AspNetUsers ||--o{ AspNetUserRoles : UserId
    AspNetRoles ||--o{ AspNetUserRoles : RoleId
    AspNetUsers ||--o{ AspNetUserClaims : UserId
    AspNetUsers ||--o{ AspNetUserLogins : UserId
    AspNetUsers ||--o{ AspNetUserTokens : UserId
    AspNetRoles ||--o{ AspNetRoleClaims : RoleId
    Wallet ||--o{ LedgerEntry : WalletId
    Wallet ||--o{ Transfer : FromWalletId
    Wallet ||--o{ Transfer : ToWalletId
    Transfer |o--o{ LedgerEntry : TransferId
```

EF-History: `__EFMigrationsHistory` (Ledger) und `__EFMigrationsHistoryIdentity` (Identity). `IdempotencyRecord.Key` unique (Replay → **409**). `LinkedInstrument` nur Last-four — kein PAN.

---

## Was heute klickbar ist

Dieselben acht Vorgänge auf der Site und in der App. Sprachen: **Türkçe (Standard), English, Deutsch, Français**. Flutter-UI Standard: Türkçe. Kein 9. Bildschirm.

| Vorgang | Website | Flutter-App |
|---------|---------|-------------|
| Anmelden | [`/giris`](http://localhost:5153/giris) | Giriş — `POST /api/token` |
| Registrieren | [`/kayit`](http://localhost:5153/kayit) | Hesap oluştur — `POST /api/register` |
| Übersicht | [`/`](http://localhost:5153/) | Özet, Pull-to-refresh — `GET /api/wallet` |
| Überweisung | [`/havale`](http://localhost:5153/havale) | Havale + Bestätigung — `POST /api/transfers` + `Idempotency-Key` |
| Aufladen / Abheben | [`/yukle-cek`](http://localhost:5153/yukle-cek) | Yükle / Çek — `POST /api/topup` / `withdraw` |
| Bewegungen | [`/hareketler`](http://localhost:5153/hareketler) | Hareketler + Filter — `GET /api/movements` |
| Beleg | [`/dekont/{id}`](http://localhost:5153/hareketler) | Dekont — `GET /api/receipts/{id}` |
| Admin | [`/admin`](http://localhost:5153/admin) | Admin-Tab (Rolle Admin) — `/api/admin/*` |

Dev: `admin@clearpay.test` / `Deneme123`. Transfer **201** / Replay **409**. OpenAPI: [http://localhost:5153/swagger](http://localhost:5153/swagger). Mobile-README: [`mobile/clearpay/README.md`](mobile/clearpay/README.md).

Redis cached nur das Özet-DTO (~60s). Kasse bleibt SQL Server. Rabbit `clearpay.outbox`, wenn `ConnectionStrings:RabbitMq` gesetzt. **Keine öffentliche Azure-URL** — du klickst `az login` (`docs/CANLI.md`).

## Interview (drei Sätze)

1. Derselbe `Idempotency-Key` ist dieselbe Absicht: der zweite HTTP ist **409 Conflict**, damit ein Timeout-Retry nicht zweimal abbucht.
2. Soll, Haben, Transfer, Idempotenz, Audit und Outbox in **einer SQL-Transaktion**; kein `UPDATE Balance` — Saldo ist `LedgerPair.NetOf`.
3. Die Outbox-Zeile steht in derselben Transaktion; Timeout verliert die Nachricht nicht. Hangfire (und Rabbit, wenn gebunden) publiziert nach dem Commit.

---

## Starten

.NET-8-SDK. **Web Development** nutzt SQL Server LocalDB — `(localdb)\MSSQLLocalDB` / `ClearPay` — für Identity und Ledger. Docker Desktop ist optional (SQL Server 2022 + Redis/Rabbit).

```bash
dotnet run --project src/ClearPay.Web --launch-profile http
```

[http://localhost:5153](http://localhost:5153). Dasselbe Geld in Flutter (**cmd**):

```bat
cd /d mobile\clearpay
flutter doctor
flutter run -d windows
```

Android-Emulator: `http://10.0.2.2:5153`. Flutter spricht JWT mit demselben Host; `firebase_core` (`clearpay-c0485`) speichert keinen Saldo. Ohne LocalDB/SQL bleibt die Übersicht `0,00 ₺`.

```bash
dotnet test
dotnet build ClearPay.slnx
```

Optionales Docker-SQL-Bind: `D:\ClearPay\data\mssql`. Lokales SA-Passwort in `.env.example` (nur Compose). `.env` nicht committen. Nicht nach Azure übernehmen.

App-Ledger ist **nur SQL Server**. MySQL (`ConnectionStrings:MySql`) ist Beifahrer, nicht die Wallet-Datenbank. Mobil **JWT → C# → SQL Server** — kein MySQL-Treiber und kein Firestore-Wallet in Flutter.

---

## Repo-Karte

```
src/ClearPay.Domain           LedgerEntry, LedgerPair, Wallet (kein Balance)
src/ClearPay.Application      IWalletReader, ITransferExecutor, IBankGateway
src/ClearPay.Infrastructure   SqlWalletReader, EF SQL Server, Identity (dieselbe LocalDB)
src/ClearPay.Web              Razor + Lokalisierung + MapControllers
mobile/clearpay               Flutter-JWT-Client (nicht in der .slnx)
tests/ClearPay.Tests          LedgerPair, Architektur, SqlWalletReader, Culture
docker-compose.yml            SQL Server 2022 — nicht die Web-App
ClearPay.slnx
```

---

## Roadmap (ehrlich)

| Fertig | Als Nächstes |
|--------|----------------|
| TASK-01…15 — Screens, Ledger, 409, Gateway, Outbox, Redis/Rabbit, Tests, Swagger | **TASK-16** — Azure App Service + Azure SQL (`az login` klickst du; keine erfundene URL) |

CI stellt `tests/ClearPay.Tests` auf `main` wieder her und testet.

---

## Docs

- [`docs/SPEC.md`](docs/SPEC.md) — Bildschirme und Geldregeln (409, eine Transaktion, Outbox)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — Onion-Schichten, zuerst Cookie, dann JWT
- [`docs/FARK.md`](docs/FARK.md) — Abgleich zuerst; kein Papara-Rivale
- [`docs/SATIS.md`](docs/SATIS.md) — 15-Sekunden-Pitch
- [`docs/DEPLOY.md`](docs/DEPLOY.md) — Compose + `dotnet run`
- [`mobile/clearpay/README.md`](mobile/clearpay/README.md) — Flutter-Client (dieselben acht Vorgänge)
- Schritt für Schritt: [`docs/OTURUM-PLAN.md`](docs/OTURUM-PLAN.md) (öffentlich im Repo). Dieselbe Liste in [Notion](https://www.notion.so/3bb31a8b18e4816bb34ffa405b4dec5d) — Share → Publish to web, damit Leute ohne Notion-Login lesen können.

Live-Ziel: Azure App Service + Azure SQL (West Europe). Heute gibt es keine `azurewebsites.net` zum Anklicken.

## Lizenz

[MIT](LICENSE) © 2026 Halil Mert Develi
