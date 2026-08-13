# ClearPay

**English** | [Türkçe](README.tr.md) | [Français](README.fr.md)

[![CI](https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml/badge.svg)](https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml)
[![MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

ASP.NET Core 8 wallet. One host: Razor Pages for the site, JSON for the API. SQL Server in Docker. Double-entry in the domain, not a `Balance` column on `Wallet`.

I am Halil Mert Develi. I wrote this as the .NET interview repo I actually want to defend — Intertech, Softtech, that kind of shop — not as a Papara clone. The UI is Turkish. Licence is MIT.

Demo. Mock `IBankGateway` for top-up and withdraw. No e-money licence, no FAST, no card acquiring, no public Azure URL. If someone sells this as a bank, they did not read the footer.

## Ledger

The usual student wallet does `wallet.Balance -= amount; SaveChanges();`. That loses the trail, races on last-write-wins, and has nothing to reverse on refund. Freeze becomes a flag taped onto a number.

Here each movement is a `LedgerEntry` pair: debit (−) and credit (+), same `PairId`, same `CorrelationId`, amounts sum to zero. Balance is `LedgerPair.NetOf`. `Wallet` has no balance field. Refund is a reverse pair; old rows stay. There is no `UPDATE Balance` helper, on purpose.

Same `Idempotency-Key` = same intent (double-click, proxy retry). First success is `201`. Replay is `409 Conflict`. A second `201` would cut the purse twice. I do not return `200` + the old body: the client then thinks a new transfer happened.

Debit, credit, `Transfer`, `IdempotencyRecord`, `AuditLog`, and `OutboxMessage` go in **one SQL commit**. The outbox row is written with the ledger; a worker publishes **after**. If HTTP times out, the intent is still in the database. Hangfire is the planned worker. It is not in the csproj yet.

That rule set lives in `src/ClearPay.Domain/Ledger`. `LedgerPair` is covered by xUnit. EF onto the Compose SQL instance is not done. `POST /api/transfers` is not an endpoint yet. Gateway classes throw `NotImplementedException`.

## What you can click today

Cookie Identity, SQLite at `App_Data/identity.db`. Register, log in, see **0,00 ₺**. That number is still hardcoded on the summary PageModel — it is not ledger net.

| Page | Route | Honest state |
|------|--------|----------------|
| Login | `/Account/Login` (`/giris`) | Works |
| Register | `/Account/Register` (`/kayit`) | Works |
| Özet | `/` | Empty summary |
| Havale | `/havale` | Form shell; Gönder is disabled |
| Yükle / Çek | `/yukle-cek` | Form shell; buttons disabled |
| Hareketler | `/hareketler` | Empty table; filters disabled |
| Dekont | — | Not built |
| Admin | — | Not built (nav hides it) |

`GET /api/health` → `{ "status": "ok", "product": "ClearPay" }`. No JWT, no Swagger, no Hangfire package, no Redis, no RabbitMQ.

`docker compose` runs SQL Server 2022 on `localhost,1433`. The web app does not open that database yet. Identity does not need Docker.

## Run

.NET 8 SDK. Docker Desktop if you want the SQL container.

```bash
docker compose up -d
dotnet run --project src/ClearPay.Web --launch-profile http
```

Open [http://localhost:5153](http://localhost:5153).

```bash
dotnet test
dotnet build ClearPay.slnx
```

Local SA password is in `.env.example` (`ClearPay_Dev1!`). Docker only. Do not commit `.env`. Do not put that password on Azure.

CI (when the workflow is on `main`) restores and tests `tests/ClearPay.Tests`.

## Layout

```
src/ClearPay.Domain           LedgerEntry, LedgerPair, Wallet (no Balance)
src/ClearPay.Application      ports (ITransferExecutor, IBankGateway, …), FluentValidation
src/ClearPay.Infrastructure   Identity (SQLite), gateway stubs that throw
src/ClearPay.Web              Razor + MapControllers, http profile :5153
tests/ClearPay.Tests          LedgerPair + auth/page smoke
docker-compose.yml            SQL Server 2022 — not the web app
ClearPay.slnx
```

Web → Application + Infrastructure. Infrastructure → Application → Domain. Domain does not reference EF or ASP.NET.

## Docs

- [`docs/SPEC.md`](docs/SPEC.md) — screens and money rules (409, one transaction, outbox)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — layers, cookie then JWT
- [`docs/FARK.md`](docs/FARK.md) — reconciliation-first; not a Papara rival
- [`docs/DEPLOY.md`](docs/DEPLOY.md) — Compose + `dotnet run`

Azure App Service + Azure SQL (West Europe) is the live target. I open the subscription; there is no `azurewebsites.net` to click today.

## License

[MIT](LICENSE) © 2026 Halil Mert Develi
