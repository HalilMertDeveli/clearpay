# ClearPay

| [English](./README.md) | [Türkçe](./README.tr.md) | [Deutsch](./README.de.md) | **Français** |
|:---------------------:|:-----------------------:|:------------------------:|:------------:|

<p align="center">

[English](./README.md) · [Türkçe](./README.tr.md) · [Deutsch](./README.de.md) · <strong>Français</strong>

</p>

<p align="center">
  <img src="docs/assets/clearpay-mark.png" width="96" alt="Marque ClearPay">
</p>

<p align="center">
  <a href="https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml"><img src="https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml/badge.svg" alt="CI"></a>
  <img src="https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/Flutter-app_mobile-02569B?logo=flutter" alt="Application Flutter">
  <img src="https://img.shields.io/badge/Android%20%7C%20Windows%20%7C%20iOS-livr%C3%A9-0F766E" alt="Android Windows iOS">
  <img src="https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver" alt="SQL Server">
  <img src="https://img.shields.io/badge/UI-TR%20%7C%20EN%20%7C%20DE%20%7C%20FR-1B2A4A" alt="Langues UI">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT"></a>
</p>

<p align="center">
  <img src="docs/assets/clearpay-hero.png" alt="ClearPay — portefeuille démo, ASP.NET Core 8, Flutter, un grand livre SQL. Pas de UPDATE Balance." width="920">
</p>

<p align="center"><b>Démo — passerelle fictive pour les recharges.</b> Pas un établissement e-money licencié. Pas Papara / FAST / une fausse banque de détail. Pas de <code>UPDATE Balance</code>.</p>

<p align="center">
  <img src="docs/assets/clearpay-rules.png" alt="Solde dérivé ; rejeu 409 ; une transaction SQL" width="920">
</p>

---

## Site web

Razor Pages sur [http://localhost:5153](http://localhost:5153) (graine Development `admin@clearpay.test` / `Deneme123`). Un nom d’hôte App Service Canada Central existe, mais `/api/health` renvoie encore **404** — le HTTPS public est **TASK-16**. Ces captures sont **locales**. Pas une UI de banque licenciée.

| Connexion `/giris` | Synthèse après connexion |
|:------------------:|:------------------------:|
| <img src="docs/assets/shot-giris.png" alt="Connexion du site ClearPay" width="420"> | <img src="docs/assets/shot-ozet.png" alt="Synthèse du site ClearPay" width="420"> |
| Barre TR · EN · DE · FR. Portefeuille démo. | Même grand livre SQL. Solde = `LedgerPair.NetOf`. |

| Inscription `/kayit` | Cartes `/kartlar` |
|:--------------------:|:-----------------:|
| <img src="docs/assets/shot-kayit.png" alt="Inscription du site ClearPay" width="420"> | <img src="docs/assets/shot-kartlar.png" alt="Cartes du site ClearPay" width="420"> |
| Identity cookie. Les quatre langues. | Quatre derniers + schéma seulement. Pas de PAN en SQL. Passerelle fictive. |

---

## Application mobile

Client Flutter JWT sur l’émulateur Android `emulator-5554` → `http://10.0.2.2:5153`. Huit opérations, même SQL. **Pas** une caisse Hive / Firestore. Firestore n’écrit que `app_meta/ping`.

<p align="center">
  <img src="docs/assets/shot-mobile.png" alt="Synthèse Flutter ClearPay sur émulateur Android" width="280">
</p>

<p align="center"><i>Özet — bandeau de langues, pied démo. Les lignes viennent de JWT → SQL (spinner tant que l’API locale répond).</i></p>

<p align="center">
  <img src="docs/assets/clearpay-clients.png" alt="Site Razor cookie vs Flutter JWT — un grand livre SQL" width="840">
</p>

---

## Web + mobile (livré)

Ce dépôt **n’est pas que le site**. L’**application Flutter** est dans [`mobile/clearpay`](mobile/clearpay) et parle au même hôte ASP.NET Core 8. **Un grand livre SQL**, **pas** de second solde sur le téléphone. Site en plus : [`/kartlar`](http://localhost:5153/kartlar) (carte démo, 4 derniers, pas de PAN). Firestore n’écrit que `app_meta/ping` — **pas** la caisse. Détail : [`README.md`](README.md) et [`mobile/clearpay/README.md`](mobile/clearpay/README.md).

**Un portefeuille, deux clients.** La même personne se connecte, vire, recharge et ouvre le reçu **sur le site** et **dans l’app Flutter**. Razor Pages (cookie) ; JSON (JWT). La partie double est dans le Domain — `Wallet` n’a **pas** de colonne `Balance`.

Je suis Halil Mert Develi. J’ai écrit ça pour un entretien .NET (Intertech, Softtech), pas pour cloner Papara. Licence MIT.

---

## Ce qui est construit

<p align="center">
  <img src="docs/assets/clearpay-layers.png" alt="Couches Clean Architecture ClearPay" width="840">
</p>

Le Web ne calcule pas le grand livre. La synthèse demande `IWalletReader`. Aujourd’hui l’adaptateur est `SqlWalletReader` : solde = `LedgerPair.NetOf`, mois entrées/sorties, cinq dernières lignes, badge gel. Si SQL Server est down, le site s’ouvre quand même — des zéros, pas un 500.

<p align="center">
  <img src="docs/assets/clearpay-ledger.png" alt="Paire en partie double ClearPay" width="840">
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

| Couche | Projet | Contient | Ne contient pas |
|--------|--------|----------|-----------------|
| UI + hôte | `ClearPay.Web` | Razor, cookie, culture, `:5153` | Net du ledger, `UPDATE Balance` |
| Cas d’usage | `ClearPay.Application` | Ports, DTO, FluentValidation | Chaînes de connexion |
| Adaptateurs | `ClearPay.Infrastructure` | EF SQL Server (Identity + ledger, même LocalDB), stubs gateway | Razor / CSS |
| Règles | `ClearPay.Domain` | `LedgerPair`, `Wallet` (pas de champ solde) | EF, HTTP, ASP.NET |

Les dépendances pointent **vers l’intérieur**. Le Domain ne référence ni EF ni ASP.NET.

---

## Schéma relationnel (SQL Server)

**Demo — sahte banka gateway. Lisanslı e-para değil.** Pas Papara / FAST / une fausse banque de détail. Huit écrans, pas un neuvième.

Développement local : `(localdb)\MSSQLLocalDB` / base `ClearPay`. Identity et le grand livre partagent cette base (deux contextes EF, deux tables d’historique). **Deux clients, un seul grand livre SQL :** Razor (cookie) et Flutter (JWT). Flutter `firebase_core` projet `clearpay-c0485` — pas un portefeuille Firestore. MySQL (`ConnectionStrings:MySql`) est un sidecar ; l’argent n’y vit pas.

Il n’y a **pas** de colonne `Wallet.Balance`. Le solde = `LedgerPair.NetOf` (C#, pas une table). `UPDATE Balance` est interdit. `Wallet.UserId` est unique et correspond à `AspNetUsers.Id` dans la même base ; **pas de FK** (deux DbContexts). FK réels : Identity plus `LedgerEntry` → `Wallet` / `Transfer` et `Transfer` → `Wallet`.

Le même mermaid est dans le README GitHub par défaut (`README.md`, section **Relational schema (SQL Server)**).

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

Historique EF : `__EFMigrationsHistory` (ledger) et `__EFMigrationsHistoryIdentity` (Identity). `IdempotencyRecord.Key` unique (rejeu → **409**). `LinkedInstrument` : quatre derniers chiffres seulement — pas de PAN.

---

## Ce qui s’ouvre aujourd’hui

Les huit mêmes opérations sur le site et dans l’app. Langues : **Türkçe (défaut), English, Deutsch, Français**. UI Flutter par défaut : Türkçe. Pas un 9ᵉ écran.

| Opération | Site | App Flutter |
|-----------|------|-------------|
| Connexion | [`/giris`](http://localhost:5153/giris) | Giriş — `POST /api/token` |
| Inscription | [`/kayit`](http://localhost:5153/kayit) | Hesap oluştur — `POST /api/register` |
| Synthèse | [`/`](http://localhost:5153/) | Özet, pull-to-refresh — `GET /api/wallet` |
| Virement | [`/havale`](http://localhost:5153/havale) | Havale + confirmation — `POST /api/transfers` + `Idempotency-Key` |
| Recharger / retirer | [`/yukle-cek`](http://localhost:5153/yukle-cek) | Yükle / Çek — `POST /api/topup` / `withdraw` |
| Mouvements | [`/hareketler`](http://localhost:5153/hareketler) | Hareketler + filtre — `GET /api/movements` |
| Reçu | [`/dekont/{id}`](http://localhost:5153/hareketler) | Dekont — `GET /api/receipts/{id}` |
| Admin | [`/admin`](http://localhost:5153/admin) | Onglet Admin (rôle Admin) — `/api/admin/*` |

Dev : `admin@clearpay.test` / `Deneme123`. Virement **201** / rejeu **409**. OpenAPI : [http://localhost:5153/swagger](http://localhost:5153/swagger). README mobile : [`mobile/clearpay/README.md`](mobile/clearpay/README.md).

Redis cache uniquement le DTO résumé (~60 s). La caisse reste SQL Server. Rabbit `clearpay.outbox` si `ConnectionStrings:RabbitMq`. **Pas d’URL Azure publique** — tu cliques `az login` (`docs/CANLI.md`).

## Entretien (trois phrases)

1. La même `Idempotency-Key` est la même intention : le second HTTP est **409 Conflict**, un retry après timeout ne débite pas deux fois.
2. Débit, crédit, transfert, idempotence, audit et outbox dans **une transaction SQL** ; pas d’`UPDATE Balance` — le solde est `LedgerPair.NetOf`.
3. La ligne outbox est écrite dans cette transaction ; un timeout ne perd pas le message. Hangfire (et Rabbit s’il est lié) publie après le commit.

---

## Lancer

SDK .NET 8. **Web Development** utilise SQL Server LocalDB — `(localdb)\MSSQLLocalDB` / `ClearPay` — pour Identity et le grand livre. Docker Desktop est optionnel (SQL Server 2022 + Redis/Rabbit).

```bash
dotnet run --project src/ClearPay.Web --launch-profile http
```

[http://localhost:5153](http://localhost:5153). Le même argent dans Flutter (**cmd**) :

```bat
cd /d mobile\clearpay
flutter doctor
flutter run -d windows
```

Émulateur Android : `http://10.0.2.2:5153`. Flutter parle JWT au même hôte ; `firebase_core` (`clearpay-c0485`) ne stocke pas le solde. Sans LocalDB/SQL, la synthèse reste `0,00 ₺`.

```bash
dotnet test
dotnet build ClearPay.slnx
```

Bind Docker SQL optionnel : `D:\ClearPay\data\mssql`. Mot de passe SA local : `.env.example` (Compose uniquement). Ne pas committer `.env`. Ne pas le réutiliser sur Azure.

Le grand livre de l’app est **SQL Server seulement**. MySQL (`ConnectionStrings:MySql`) est un sidecar, pas la base du portefeuille. Mobile **JWT → C# → SQL Server** — pas de driver MySQL ni de portefeuille Firestore dans Flutter.

---

## Carte du dépôt

```
src/ClearPay.Domain           LedgerEntry, LedgerPair, Wallet (pas de Balance)
src/ClearPay.Application      IWalletReader, ITransferExecutor, IBankGateway
src/ClearPay.Infrastructure   SqlWalletReader, EF SQL Server, Identity (même LocalDB)
src/ClearPay.Web              Razor + localisation + MapControllers
mobile/clearpay               client Flutter JWT (pas dans le .slnx)
tests/ClearPay.Tests          LedgerPair, architecture, SqlWalletReader, culture
docker-compose.yml            SQL Server 2022 — pas l’app web
ClearPay.slnx
```

---

## Feuille de route (honnête)

| Fait | Ensuite |
|------|---------|
| TASK-01…15 — écrans, ledger, 409, gateway, outbox, Redis/Rabbit, tests, Swagger | **TASK-16** — Azure App Service + Azure SQL (`az login` à toi ; pas d’URL inventée) |

La CI restore et teste `tests/ClearPay.Tests` sur `main`.

---

## Docs

- [`docs/YOL.md`](docs/YOL.md) — à quoi ça sert, où ça va (carrière d’abord ; URL live = TASK-16)
- [`docs/SPEC.md`](docs/SPEC.md) — écrans et règles d’argent (409, une transaction, outbox)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — couches onion, cookie puis JWT
- [`docs/FARK.md`](docs/FARK.md) — rapprochement d’abord ; pas un rival Papara
- [`docs/SATIS.md`](docs/SATIS.md) — pitch 15 secondes
- [`docs/DEPLOY.md`](docs/DEPLOY.md) — Compose + `dotnet run`
- [`mobile/clearpay/README.md`](mobile/clearpay/README.md) — client Flutter (les huit mêmes opérations)
- Pas à pas : [`docs/OTURUM-PLAN.md`](docs/OTURUM-PLAN.md) (public dans ce dépôt). Même liste sur [Notion](https://www.notion.so/3bb31a8b18e4816bb34ffa405b4dec5d) — Share → Publish to web pour les lecteurs sans compte Notion.

Cible live : Azure App Service + Azure SQL (West Europe). Pas d’`azurewebsites.net` à cliquer aujourd’hui.

## Licence

[MIT](LICENSE) © 2026 Halil Mert Develi
