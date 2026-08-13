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

## Status

Scaffold and agent workflow are in `docs/`. Application code starts at **TASK-02**. Run the next task from `docs/TASKS.md`.

## Docs

- [SPEC](docs/SPEC.md) — product, screens, money rules
- [PLAN](docs/PLAN.md) — phased work plan
- [TASKS](docs/TASKS.md) — queue
- [AGENTS](docs/AGENTS.md) — roles
- [DEPLOY](docs/DEPLOY.md) — local / Azure
