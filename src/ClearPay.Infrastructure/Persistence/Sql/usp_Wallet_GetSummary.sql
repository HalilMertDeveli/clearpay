-- Wallet summary (Özet screen): ledger net + this month's in/out in one pass.
-- Balance is never a stored column; it is SUM(Amount) over the wallet's signed ledger rows.
-- Reads only IX_LedgerEntry_WalletId_CreatedAt (key WalletId, CreatedAt; INCLUDE Amount ...):
-- one index seek on WalletId, no key lookups into the clustered index.
CREATE OR ALTER PROCEDURE dbo.usp_Wallet_GetSummary
    @WalletId   uniqueidentifier,
    @MonthStart datetimeoffset(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Balance       = ISNULL(SUM(e.Amount), 0),
        MonthOutgoing = ISNULL(SUM(CASE WHEN e.CreatedAt >= @MonthStart AND e.Amount < 0 THEN -e.Amount END), 0),
        MonthIncoming = ISNULL(SUM(CASE WHEN e.CreatedAt >= @MonthStart AND e.Amount > 0 THEN e.Amount END), 0)
    FROM dbo.LedgerEntry AS e
    WHERE e.WalletId = @WalletId;
END
