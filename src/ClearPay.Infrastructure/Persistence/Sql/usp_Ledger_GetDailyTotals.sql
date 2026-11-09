-- Day-level ledger totals per kind, for nightly reconciliation (gün sonu mutabakat).
-- Window is [@DayStart, @DayEnd). Every movement is a +/- pair, so Net per kind must be 0;
-- a non-zero Net means a half-written pair and is what a reconciliation job should alert on.
-- Reads only IX_LedgerEntry_CreatedAt (key CreatedAt; INCLUDE Kind, Amount): one range seek.
CREATE OR ALTER PROCEDURE dbo.usp_Ledger_GetDailyTotals
    @DayStart datetimeoffset(7),
    @DayEnd   datetimeoffset(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.Kind,
        EntryCount  = COUNT_BIG(*),
        CreditTotal = ISNULL(SUM(CASE WHEN e.Amount > 0 THEN e.Amount END), 0),
        DebitTotal  = ISNULL(SUM(CASE WHEN e.Amount < 0 THEN -e.Amount END), 0),
        Net         = SUM(e.Amount)
    FROM dbo.LedgerEntry AS e
    WHERE e.CreatedAt >= @DayStart
      AND e.CreatedAt <  @DayEnd
    GROUP BY e.Kind
    ORDER BY e.Kind;
END
