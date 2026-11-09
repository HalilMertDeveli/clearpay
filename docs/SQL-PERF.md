# SQL-PERF — ledger read queries, stored procedures and indexes

Scope: the **existing ledger / wallet tables** (`Wallet`, `LedgerEntry`). Payment tables are not touched here.
Every number below was **measured** on a throwaway SQL Server 2022 container (Developer Edition, RTM-CU27-GDR, Linux, Docker Desktop on the dev laptop) with `scripts/perf/run-perf.ps1`. Raw output: [`scripts/perf/results/*.json`](../scripts/perf/results). Actual execution plans: [`docs/sqlplans/`](sqlplans) (open `.sqlplan` in SSMS or Azure Data Studio).

This is a demo dataset on a laptop, not a production benchmark. Logical reads are the stable number; milliseconds move between runs.

## What was slow and why

The old C# readers asked SQL for **every ledger row of the wallet** and did the math in memory:

| Screen | Old code path | Problem |
|--------|---------------|---------|
| Özet (summary) | `SqlWalletReader`: `LedgerEntries.Where(WalletId).ToListAsync()`, then `NetOf`, month sums, top 5 in C# | Ships all rows of the wallet over TCP on every summary. The treasury wallet has 120,000 rows in the seed. |
| Hareketler (statement) | `SqlActivityReader`: same full load, filter + sort + page in C#, then 2 more queries for counterparties | Same full load, plus `PairId` had **no index** → the counterparty lookup scans the table. |
| Gün sonu mutabakat (daily totals) | none yet | `CreatedAt` alone had no index → any "per day" query scans the clustered index. |

The only ledger index was `IX_LedgerEntry_WalletId_CreatedAt (WalletId, CreatedAt)` with **no included columns**, so every row it found needed a **Key Lookup** into the clustered index (`PK_LedgerEntry`, random `Guid` key) to read `Amount` / `Kind`.

## What changed

Migration `20261109113151_AddLedgerReadProcedures` (EF Core, SQL Server):

| Object | Definition | Used by |
|--------|------------|---------|
| `IX_LedgerEntry_WalletId_CreatedAt` | key `(WalletId, CreatedAt)` **INCLUDE** `(Amount, Kind, CorrelationId, PairId)` — same name, now covering | summary + statement procs |
| `IX_LedgerEntry_PairId` (new) | key `(PairId)` INCLUDE `(WalletId, Amount)` | counterparty of a ledger line |
| `IX_LedgerEntry_CreatedAt` (new) | key `(CreatedAt)` INCLUDE `(Kind, Amount)` | daily totals |
| `dbo.usp_Wallet_GetSummary` | `SUM(Amount)` + this month's in / out in one pass | **`SqlWalletReader`** (app) |
| `dbo.usp_Wallet_GetStatementPage` | optional from / to / kind filter, `COUNT` into a variable, `OFFSET … FETCH`, counterparty via `OUTER APPLY` after the page is cut, `OPTION (RECOMPILE)` | **`SqlActivityReader`** + last 5 on Özet (app) |
| `dbo.usp_Ledger_GetDailyTotals` | per `Kind`: count, credit, debit, net for `[DayStart, DayEnd)` | `LedgerProcedures.GetDailyTotalsAsync` — ready for the nightly reconciliation job; **not on a screen yet** |

- Procedure bodies: [`src/ClearPay.Infrastructure/Persistence/Sql/*.sql`](../src/ClearPay.Infrastructure/Persistence/Sql) — embedded resources, each one `CREATE OR ALTER PROCEDURE` (safe to re-run). The migration executes them; `Down` drops them.
- C#: [`LedgerProcedures`](../src/ClearPay.Infrastructure/Persistence/LedgerProcedures.cs) calls them with `Database.SqlQueryRaw<T>` and typed `SqlParameter`s (no string concatenation, no Dapper).
- SQLite (unit tests, `UseSqliteLedger`) has no procs: the readers keep the old LINQ path there (`LedgerProcedures.IsAvailable == Database.IsSqlServer()`).
- Still no `Balance` column and no `UPDATE`: balance stays `SUM(Amount)` over signed rows.
- No filtered index: none of these queries has a selective, constant predicate (e.g. `Status = Pending`) that would justify one. `OutboxMessage (Status, OccurredAt)` already existed and was not changed.

## Dataset

[`scripts/perf/01-seed.sql`](../scripts/perf/01-seed.sql), deterministic (integer hashing, no `RAND()`):

- 5,001 wallets (5,000 customers + 1 treasury), **600,000 `LedgerEntry` rows** = 300,000 +/− pairs, 0 broken pairs (checked by the seed).
- 30 % top-up (treasury → customer), 10 % withdraw (customer → treasury), 60 % transfer (customer → customer), over 365 days.
- The treasury wallet is the hot one: **120,000 rows**. A customer wallet has ~120 rows.
- Ids are `NEWID()` like `Guid.NewGuid()` in the app. `PK_LedgerEntry` is 10,240 pages (80 MB).

## Method

`scripts/perf/measure.ps1`, for each of three index states on the same data:

| State | LedgerEntry indexes |
|-------|---------------------|
| `no-index` | only the clustered PK (and `IX_LedgerEntry_TransferId`) |
| `before` | the pre-change schema: narrow `IX_LedgerEntry_WalletId_CreatedAt (WalletId, CreatedAt)` |
| `after` | migration applied: covering + `PairId` + `CreatedAt` indexes |

- Plan cache cleared at the start of each state (`DBCC FREEPROCCACHE`, throwaway server only). Inside a state the cache is **not** cleared: each proc is compiled for the first wallet that calls it (a customer) and reused for the treasury, as it would be in production.
- Each query: 1 warm-up, then 7 runs with `SET STATISTICS IO, TIME ON` → **logical reads** (sum over tables) and **median CPU / elapsed** of the outer statement. Then 1 run with `SET STATISTICS XML ON` → actual plan saved as `.sqlplan`.
- Warm buffer pool (no `DROPCLEANBUFFERS`), so elapsed ≈ CPU work, not disk.
- q0 is the old EF query exactly as EF sends it (`sp_executesql`, all columns, `WHERE WalletId = @p`).

## Results — before / after

Logical reads (8 KB pages) and median elapsed / CPU in ms. Plan column = how `LedgerEntry` is accessed in the **actual** plan.

| Query | no-index reads | before reads | **after reads** | before elapsed / CPU | **after elapsed / CPU** | before plan | after plan |
|-------|---------------:|-------------:|----------------:|---------------------:|------------------------:|-------------|------------|
| q1 summary, customer (120 rows) | 10,783 | 372 | **6** | 0 / 0 | **0 / 0** | Index Seek + 120 Key Lookups | Index Seek (covering) |
| q1 summary, treasury (120,000 rows) | 10,783 | **368,223** | **1,392** | 291 / 289 | **30 / 29** | Index Seek + **120,000 Key Lookups** (plan sniffed for a customer) | Index Seek (covering) |
| q2 statement p1, customer | 126,645 | 105,530 | **110** | 296 / 293 | **4 / 2** | Seek + Key Lookups; counterparty = **Clustered Index Scan × 20** | 2 × Index Seek + `IX_LedgerEntry_PairId` Seek × 20 |
| q2 statement p1, treasury | 137,981 | 127,269 | **1,495** | 319 / 362 | **15 / 14** | Clustered Index Scan (count) + counterparty scans | Index Seek (count over 120k in the covering index) + PairId Seek |
| q2 statement p3, treasury, 1 month + kind | 120,229 | 109,699 | **1,497** | 267 / 309 | **12 / 10** | Clustered Index Scan + counterparty scans | Index Seek on the date range + PairId Seek |
| q3 daily totals (1 day, 1,650 rows) | 10,783 | 10,783 | **13** | 10 / 40 | **0 / 0** | Clustered Index Scan | Index Seek on `IX_LedgerEntry_CreatedAt` |
| q0 old EF full load, treasury | 10,783 | 10,783 | 10,783 | 466 / 1,040 | 328 / 867 | Clustered Index Scan (parallel) | unchanged — not covered (`Description`, `TransferId`) |

Client-side time (wall clock in the script, including the rows over TCP, median): q0 old full load **467 ms** (before) vs q1 summary treasury **31.8 ms** + q2 statement treasury **17.1 ms** (after). The app now runs q1 + q2 instead of q0.

Plans: `docs/sqlplans/<state>-<query>.sqlplan`, e.g. [`before-q1-summary-treasury.sqlplan`](sqlplans/before-q1-summary-treasury.sqlplan) vs [`after-q1-summary-treasury.sqlplan`](sqlplans/after-q1-summary-treasury.sqlplan), [`before-q2-statement-customer-p1.sqlplan`](sqlplans/before-q2-statement-customer-p1.sqlplan) vs [`after-q2-statement-customer-p1.sqlplan`](sqlplans/after-q2-statement-customer-p1.sqlplan).

## What the numbers say

1. **Key lookups were the cost, not the seek.** With the narrow index, the treasury summary did a seek plus 120,000 key lookups = 368,223 reads — **34× more than scanning the whole table** (10,783). The covering index removes the lookups: 1,392 reads.
2. **Parameter sniffing is real here.** `usp_Wallet_GetSummary` was first compiled for a customer (120 rows → seek + lookup is cheap) and that plan was reused for the treasury. With the covering index the same plan shape is cheap for both, so sniffing stops mattering. The statement proc uses `OPTION (RECOMPILE)` because its optional filters change the best plan per call.
3. **A missing index on a join column hurts more than the main filter.** Before, the counterparty `OUTER APPLY` scanned `LedgerEntry` once per page row (~100k reads for 20 rows). `IX_LedgerEntry_PairId` makes it 20 small seeks.
4. **Indexes do not fix "load everything".** q0 stays at 10,783 reads and ~330–470 ms whatever the index; the fix was moving `SUM` / `COUNT` / paging into SQL (the procs).

## Costs and limits (honest)

- **Space** (after, 600k rows): `PK_LedgerEntry` 80.0 MB, `IX_LedgerEntry_WalletId_CreatedAt` 54.6 MB (was narrow), `IX_LedgerEntry_PairId` 37.0 MB, `IX_LedgerEntry_CreatedAt` 26.6 MB, `IX_LedgerEntry_TransferId` 22.3 MB.
- **Writes**: each ledger pair now maintains two more nonclustered indexes. A rough probe ([`02-insert-cost.sql`](../scripts/perf/02-insert-cost.sql): 3 × set-based insert of 20,000 rows, rolled back) gave 2,534 / 776 / 509 ms with the old indexes and 2,120 / 594 / 487 ms with the new ones — **inside the noise; I could not measure a write penalty reliably** with this probe. It is not a per-request OLTP benchmark.
- The treasury summary still reads 1,392 pages (all 120k rows of the hot wallet, from the narrow covering index). The next step for a hot wallet would be a daily balance snapshot table (closed days summed once), which is a design change, not an index.
- `COUNT` for the statement total still touches every matching row; fine for this demo, keyset paging without a total would be cheaper for very long histories.
- Laptop + Docker Desktop, warm cache, one run of the whole suite. Milliseconds are indicative only.

## Reproduce

Needs Docker, the .NET 8 SDK, `dotnet-ef`, and `sqlcmd` (ODBC 17/18). Uses a **throwaway** container (no volume) on port 14333 — never `D:/ClearPay/data` or a real database.

```powershell
# full run: start container, migrate to the pre-change schema, seed, measure 3 states, remove container
powershell -ExecutionPolicy Bypass -File scripts/perf/run-perf.ps1
# keep the container afterwards to poke at it
powershell -ExecutionPolicy Bypass -File scripts/perf/run-perf.ps1 -KeepContainer
```

Stored procedure tests against a real SQL Server (skipped, with a reason, when the variable is not set; CI sets it with a service container):

```powershell
$env:CLEARPAY_TEST_SQLSERVER = 'Server=localhost,14333;User Id=sa;Password=<throwaway>;TrustServerCertificate=True'
dotnet test ClearPay.slnx --filter "FullyQualifiedName~LedgerProcedures"
```
