-- Rough write-cost probe (docs/SQL-PERF.md): 3 x set-based INSERT of 20,000 LedgerEntry rows, rolled back.
-- Run manually on the throwaway perf DB once with the old indexes and once with the new ones.
SET NOCOUNT ON;
DECLARE @a uniqueidentifier = (SELECT Id FROM dbo.Wallet WHERE UserId = N'perf-user-00007');
DECLARE @b uniqueidentifier = (SELECT Id FROM dbo.Wallet WHERE UserId = N'perf-user-00008');
DECLARE @i int = 0, @t0 datetime2, @ms int, @reads bigint;
WHILE @i < 3
BEGIN
  SET @t0 = SYSDATETIME();
  BEGIN TRAN;
  INSERT dbo.LedgerEntry (Id, WalletId, Amount, PairId, CorrelationId, TransferId, Kind, Description, CreatedAt)
  SELECT TOP (20000) NEWID(), CASE WHEN n % 2 = 0 THEN @a ELSE @b END, CASE WHEN n % 2 = 0 THEN -1.00 ELSE 1.00 END,
         NEWID(), NEWID(), NULL, 0, N'insert test', DATEADD(second, n, CAST('2026-11-01T00:00:00+00:00' AS datetimeoffset(7)))
  FROM (SELECT n = ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) FROM sys.all_objects a CROSS JOIN sys.all_objects b) x;
  ROLLBACK;
  SET @ms = DATEDIFF(millisecond, @t0, SYSDATETIME());
  PRINT CONCAT('insert 20000 rows (rolled back): ', @ms, ' ms');
  SET @i += 1;
END
