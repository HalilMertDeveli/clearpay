namespace ClearPay.Application.Funding;

public enum FundingResultKind
{
    Created = 0,
    Replay = 1,
    TimedOut = 2,
    InsufficientFunds = 3,
    Frozen = 4,
    InvalidAmount = 5,
    MissingKey = 6,
    GatewayFailed = 7
}
