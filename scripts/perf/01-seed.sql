/*
  ClearPay T-SQL perf seed (docs/SQL-PERF.md).
  Run ONLY against a throwaway database (scripts/perf/run-perf.ps1 uses a temporary Docker SQL Server).
  Never run against D:/ClearPay data or any real ledger.

  Shape (deterministic, no RAND()):
    - 5,000 customer wallets  (UserId 'perf-user-00001' ...)
    - 1 treasury wallet       (UserId 'perf-treasury') - every top-up / withdraw touches it, so it is the hot wallet
    - 300,000 movements = 600,000 LedgerEntry rows (signed +/- pairs, sum per pair = 0)
        30% TopUp    treasury  -> customer
        10% Withdraw customer  -> treasury
        60% Transfer customer  -> customer
    - CreatedAt spread over 365 days before 2026-11-01 00:00 +00:00
  Ids are NEWID() like Guid.NewGuid() in the app (random clustered PK, realistic page splits).
  TransferId stays NULL: Transfer rows are not needed for the read queries measured here.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Wallets int = 5000;
DECLARE @Pairs   int = 300000;
DECLARE @Anchor  datetimeoffset(7) = '2026-11-01T00:00:00+00:00';

-- Re-runnable: remove previous perf rows only.
DELETE e FROM dbo.LedgerEntry AS e
JOIN dbo.Wallet AS w ON w.Id = e.WalletId
WHERE w.UserId LIKE N'perf-%';
DELETE FROM dbo.Wallet WHERE UserId LIKE N'perf-%';

-- Number source (0 .. N-1) without loops.
DROP TABLE IF EXISTS #n;
SELECT TOP (@Pairs) n = CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS bigint)
INTO #n
FROM sys.all_objects AS a
CROSS JOIN sys.all_objects AS b
CROSS JOIN sys.all_objects AS c;

-- Wallets: ordinal 0 = treasury, 1..@Wallets = customers.
DROP TABLE IF EXISTS #w;
CREATE TABLE #w (Ordinal int NOT NULL PRIMARY KEY, Id uniqueidentifier NOT NULL);

INSERT #w (Ordinal, Id)
SELECT CAST(n AS int), NEWID() FROM #n WHERE n <= @Wallets;

INSERT dbo.Wallet (Id, UserId, IsFrozen, CreatedAt)
SELECT
    Id,
    CASE WHEN Ordinal = 0 THEN N'perf-treasury'
         ELSE N'perf-user-' + RIGHT(N'00000' + CAST(Ordinal AS nvarchar(10)), 5) END,
    0,
    DATEADD(day, -400, @Anchor)
FROM #w;

-- Movements: kind, from/to ordinal, amount, timestamp derived from n with cheap integer hashing.
DROP TABLE IF EXISTS #p;
SELECT
    n,
    PairId        = NEWID(),
    CorrelationId = NEWID(),
    Kind          = CASE WHEN n % 10 < 3 THEN 1       -- TopUp
                         WHEN n % 10 = 3 THEN 2       -- Withdraw
                         ELSE 0 END,                  -- Transfer
    A             = CAST(1 + (n * 7919) % @Wallets AS int),
    B             = CAST(1 + (n * 104729 + 17) % @Wallets AS int),
    Amount        = CAST(1 + (n * 31337) % 250000 AS decimal(18, 2)) / 100,
    CreatedAt     = DATEADD(second, -CAST((n * 2654435761) % 31536000 AS int), @Anchor)
INTO #p
FROM #n;

-- Transfer to self is not a movement: shift B.
UPDATE #p SET B = 1 + (B % @Wallets) WHERE Kind = 0 AND A = B;

DROP TABLE IF EXISTS #legs;
SELECT
    p.PairId, p.CorrelationId, p.Kind, p.Amount, p.CreatedAt,
    FromOrdinal = CASE p.Kind WHEN 1 THEN 0 ELSE p.A END,
    ToOrdinal   = CASE p.Kind WHEN 2 THEN 0 WHEN 1 THEN p.A ELSE p.B END
INTO #legs
FROM #p AS p;

BEGIN TRAN;

-- Debit leg (-Amount) and credit leg (+Amount) of every pair.
INSERT dbo.LedgerEntry (Id, WalletId, Amount, PairId, CorrelationId, TransferId, Kind, Description, CreatedAt)
SELECT NEWID(), wf.Id, -l.Amount, l.PairId, l.CorrelationId, NULL, l.Kind, N'perf seed', l.CreatedAt
FROM #legs AS l
JOIN #w AS wf ON wf.Ordinal = l.FromOrdinal
UNION ALL
SELECT NEWID(), wt.Id, l.Amount, l.PairId, l.CorrelationId, NULL, l.Kind, N'perf seed', l.CreatedAt
FROM #legs AS l
JOIN #w AS wt ON wt.Ordinal = l.ToOrdinal;

COMMIT;

UPDATE STATISTICS dbo.LedgerEntry WITH FULLSCAN;
UPDATE STATISTICS dbo.Wallet WITH FULLSCAN;

-- Sanity: every pair nets to zero, row counts.
SELECT
    Wallets      = (SELECT COUNT(*) FROM dbo.Wallet WHERE UserId LIKE N'perf-%'),
    LedgerRows   = (SELECT COUNT(*) FROM dbo.LedgerEntry),
    TreasuryRows = (SELECT COUNT(*) FROM dbo.LedgerEntry AS e JOIN dbo.Wallet AS w ON w.Id = e.WalletId WHERE w.UserId = N'perf-treasury'),
    BrokenPairs  = (SELECT COUNT(*) FROM (SELECT PairId FROM dbo.LedgerEntry GROUP BY PairId HAVING SUM(Amount) <> 0) AS x);
