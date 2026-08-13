# ClearPay

[English](README.md) | [Türkçe](README.tr.md) | **Français**

[![CI](https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml/badge.svg)](https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml)
[![MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Portefeuille ASP.NET Core 8. Un seul hôte : Razor Pages pour le site, JSON pour l’API. SQL Server dans Docker. Partie double dans le Domain — pas de colonne `Balance` sur `Wallet`.

Je suis Halil Mert Develi. J’ai écrit ça pour un entretien .NET (Intertech, Softtech, ce type de boîte), pas pour cloner Papara. L’UI est en turc. Licence MIT.

Démo. `IBankGateway` fictif pour recharger / retirer. Pas de licence e-money, pas de FAST, pas d’acquisition carte, pas d’URL Azure publique. Ce n’est pas une banque ; le pied de page le dit déjà.

## Grand livre

Le portefeuille étudiant classique : `wallet.Balance -= amount; SaveChanges();`. Pas de trace, last-write-wins, rien à inverser pour un remboursement. Le gel devient un booléen collé sur un nombre.

Ici chaque mouvement est une paire `LedgerEntry` : débit (−) et crédit (+), même `PairId`, même `CorrelationId`, somme nulle. Le solde est `LedgerPair.NetOf`. `Wallet` n’a pas de champ solde. Un remboursement = paire inverse ; les anciennes lignes restent. Pas d’helper `UPDATE Balance`. C’est volontaire.

Même `Idempotency-Key` = même intention (double-clic, retry proxy). Premier succès : `201`. Rejeu : `409 Conflict`. Un second `201` débiterait deux fois. Je ne renvoie pas `200` + l’ancien body : le client croirait à un nouveau virement.

Débit, crédit, `Transfer`, `IdempotencyRecord`, `AuditLog`, `OutboxMessage` : **un seul commit SQL**. La ligne outbox part avec le grand livre ; un worker publie **après**. Timeout HTTP : l’intention est toujours en base. Hangfire est le worker prévu. Il n’est pas dans le csproj.

Les règles sont dans `src/ClearPay.Domain/Ledger`. `LedgerPair` a des tests xUnit. Le mapping EF vers le SQL Compose n’est pas fait. `POST /api/transfers` n’existe pas. Les classes gateway lèvent `NotImplementedException`.

## Ce qui s’ouvre aujourd’hui

Identity cookie, SQLite : `App_Data/identity.db`. Inscription, connexion, **0,00 ₺**. Ce chiffre est encore en dur dans le PageModel — ce n’est pas le net du grand livre.

| Page | Route | État |
|------|--------|------|
| Connexion | `/Account/Login` (`/giris`) | OK |
| Inscription | `/Account/Register` (`/kayit`) | OK |
| Özet | `/` | Synthèse vide |
| Havale | `/havale` | Formulaire ; Gönder désactivé |
| Yükle / Çek | `/yukle-cek` | Formulaire ; boutons désactivés |
| Hareketler | `/hareketler` | Table vide ; filtres off |
| Dekont | — | Pas encore |
| Admin | — | Pas encore (menu caché) |

`GET /api/health` → `{ "status": "ok", "product": "ClearPay" }`. Pas de JWT, pas de Swagger, pas de paquet Hangfire, pas de Redis, pas de RabbitMQ.

`docker compose` lance SQL Server 2022 sur `localhost,1433`. L’app web ne lit pas encore cette base. Identity n’a pas besoin de Docker.

## Lancer

SDK .NET 8. Docker Desktop pour le conteneur SQL.

```bash
docker compose up -d
dotnet run --project src/ClearPay.Web --launch-profile http
```

[http://localhost:5153](http://localhost:5153)

```bash
dotnet test
dotnet build ClearPay.slnx
```

Mot de passe SA local : `.env.example` (`ClearPay_Dev1!`). Docker uniquement. Ne pas committer `.env`. Ne pas le réutiliser sur Azure.

La CI (workflow sur `main`) restore et teste `tests/ClearPay.Tests`.

## Dépôt

```
src/ClearPay.Domain           LedgerEntry, LedgerPair, Wallet (pas de Balance)
src/ClearPay.Application      ports (ITransferExecutor, IBankGateway, …), FluentValidation
src/ClearPay.Infrastructure   Identity (SQLite), stubs gateway qui throw
src/ClearPay.Web              Razor + MapControllers, profil http :5153
tests/ClearPay.Tests          LedgerPair + smoke auth/pages
docker-compose.yml            SQL Server 2022 — pas l’app web
ClearPay.slnx
```

Web → Application + Infrastructure. Infrastructure → Application → Domain. Le Domain ne référence ni EF ni ASP.NET.

## Docs

- [`docs/SPEC.md`](docs/SPEC.md) — écrans et règles d’argent (409, une transaction, outbox)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — couches, cookie puis JWT
- [`docs/FARK.md`](docs/FARK.md) — rapprochement d’abord ; pas un rival Papara
- [`docs/DEPLOY.md`](docs/DEPLOY.md) — Compose + `dotnet run`

Cible live : Azure App Service + Azure SQL (West Europe). C’est moi qui ouvre l’abonnement ; pas d’`azurewebsites.net` à cliquer aujourd’hui.

## Licence

[MIT](LICENSE) © 2026 Halil Mert Develi
