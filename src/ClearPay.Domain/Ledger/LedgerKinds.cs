namespace ClearPay.Domain.Ledger;

/// <summary>What the signed pair represents. Refund is a reverse pair, not a balance UPDATE.</summary>
public enum LedgerEntryKind
{
    Transfer = 0,
    TopUp = 1,
    Withdraw = 2,
    Refund = 3
}

/// <summary>Transfer row status. Gateway pending/fail arrives in TASK-07; TASK-06 posts Completed in one commit.</summary>
public enum TransferStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2
}
