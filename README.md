# ClearPay

ASP.NET Core 8 digital wallet (demo). Idempotent P2P transfers, double-entry ledger on SQL Server, mock bank gateway (REST + SOAP), outbox so a timeout does not lose a payment.

**Not a real bank.** No live POS, FAST, or card acquiring.

## Stack

- C# 12 / .NET 8 / ASP.NET Core (Razor Pages + Web API)
- SQL Server, EF Core, Dapper
- ASP.NET Identity (cookie) + JWT (API)
- Serilog, xUnit, Docker Compose
- Hangfire + outbox; Redis and RabbitMQ in a later phase

## Interview story this repo proves

| Why | What the code does |
|-----|--------------------|
| **409** | Same `Idempotency-Key` means the same intent. A second success would debit twice. |
| **Transaction** | Debit, credit, idempotency row, audit, and outbox commit together or not at all. |
| **Outbox** | The ledger write is the source of truth; the message is published after commit so HTTP timeout cannot drop it. |

## CV bullets (intended)

- Built ClearPay, an ASP.NET Core 8 wallet with idempotent P2P transfers, JWT/cookie auth, and a double-entry ledger on SQL Server.
- Integrated a mock bank gateway over REST and SOAP; used an outbox + queue so payment completion is not lost on timeout.
- Shipped Docker Compose, xUnit tests, Serilog correlation, and CI/CD to Azure App Service.

## Run locally

```bash
docker compose up -d
dotnet run --project src/ClearPay.Web --launch-profile http
```

Open http://localhost:5153 — left nav: Özet, Havale, Yükle/Çek, Hareketler. Admin is hidden until TASK-10. SQL is up; the app does not read it yet (TASK-04).

## Status

TASK-02 skeleton is in the repo. Next: **TASK-03** login, register, empty wallet. Queue: `docs/TASKS.md`.

## Docs

- [SPEC](docs/SPEC.md) — product, screens, money rules
- [PLAN](docs/PLAN.md) — phased work plan
- [Çalışma planı](docs/CALISMA-PLANI.md) — agent sequence + test gates (Turkish)
- [Yönetici raporu](docs/YONETICI-RAPORU.md) — status / RAG (Turkish)
- [Tartışma](docs/TARTISMA.md) — discuss then act (Turkish; required before `src/` changes)
- [TASKS](docs/TASKS.md) — queue
- [AGENTS](docs/AGENTS.md) — roles
- [DEPLOY](docs/DEPLOY.md) — local / Azure
- [Öğrenme](docs/OGRENME.md) — neden böyle (Turkish)
- [Kronik](docs/KRONIK.md) — start-to-now learning chronicle (Turkish)
- [Senin işlerin](docs/SENIN-ISLERIN.md) — human-only checklist
- [Ödeme (senin)](docs/ODEME-SENIN.md) — demo payment: what you do / don’t do (Turkish)
- [SATIS](docs/SATIS.md) — interview pitch / CV (Turkish)
- [FARK](docs/FARK.md) — reconciliation-first ledger; not a Papara rival (Turkish)
- [FINANS](docs/FINANS.md) — double-entry, why not UPDATE Balance, correlation id, demo money, finance interview Q&A (Turkish)
- [İK](docs/IK.md) — candidate CV, 15/30 min script, target firms (Turkish; not hiring)
- [PR](docs/PR.md) — live URL + honest ranking (Turkish; not #1 for havale/Papara)
- [PAZARLAMA](docs/PAZARLAMA.md) — GitHub, LinkedIn, demo URL (Turkish; no Papara-competitor ads)
- [DESTEK](docs/DESTEK.md) — demo FAQ (register, 409, timeout, local without Azure; not a bank helpdesk)
