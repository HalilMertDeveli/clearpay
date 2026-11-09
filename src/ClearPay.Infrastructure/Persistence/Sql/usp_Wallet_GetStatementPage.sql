-- Statement / movements page (Hareketler screen + "last 5" on Özet).
-- Optional filters: @From (inclusive), @To (exclusive), @Kind (LedgerEntryKind int).
-- OFFSET/FETCH paging, newest first. TotalCount is computed once into a variable (no window spool).
-- The page (max 50 rows) is cut first; only then the counterparty is resolved via IX_LedgerEntry_PairId,
-- in the same round trip (the old C# path loaded every row of the wallet and issued extra queries).
-- OPTION (RECOMPILE): the catch-all "@x IS NULL OR ..." predicates are folded away per call,
-- so the optimizer can seek on (WalletId, CreatedAt) for the actual date range.
CREATE OR ALTER PROCEDURE dbo.usp_Wallet_GetStatementPage
    @WalletId uniqueidentifier,
    @From     datetimeoffset(7) = NULL,
    @To       datetimeoffset(7) = NULL,
    @Kind     int               = NULL,
    @Offset   int               = 0,
    @PageSize int               = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Offset < 0 SET @Offset = 0;
    IF @PageSize < 1 SET @PageSize = 1;
    IF @PageSize > 50 SET @PageSize = 50;

    DECLARE @Total int;

    SELECT @Total = COUNT(*)
    FROM dbo.LedgerEntry AS e
    WHERE e.WalletId = @WalletId
      AND (@From IS NULL OR e.CreatedAt >= @From)
      AND (@To   IS NULL OR e.CreatedAt <  @To)
      AND (@Kind IS NULL OR e.Kind = @Kind)
    OPTION (RECOMPILE);

    WITH page AS
    (
        SELECT e.Id, e.CreatedAt, e.Kind, e.Amount, e.CorrelationId, e.PairId
        FROM dbo.LedgerEntry AS e
        WHERE e.WalletId = @WalletId
          AND (@From IS NULL OR e.CreatedAt >= @From)
          AND (@To   IS NULL OR e.CreatedAt <  @To)
          AND (@Kind IS NULL OR e.Kind = @Kind)
        ORDER BY e.CreatedAt DESC, e.Id DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    )
    SELECT
        p.Id,
        p.CreatedAt,
        p.Kind,
        p.Amount,
        p.CorrelationId,
        p.PairId,
        CounterpartyUserId = cw.UserId,
        TotalCount         = @Total
    FROM page AS p
    OUTER APPLY
    (
        SELECT TOP (1) c.WalletId
        FROM dbo.LedgerEntry AS c
        WHERE c.PairId = p.PairId
          AND c.WalletId <> @WalletId
    ) AS cp
    LEFT JOIN dbo.Wallet AS cw ON cw.Id = cp.WalletId
    ORDER BY p.CreatedAt DESC, p.Id DESC
    OPTION (RECOMPILE);
END
