using ClearPay.Application.Banking;

namespace ClearPay.Application.Funding;

public sealed record FundingCommand(
    string ActorUserId,
    BankOperation Operation,
    decimal Amount,
    string AccountHint,
    string IdempotencyKey);
