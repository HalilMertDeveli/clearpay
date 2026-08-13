namespace ClearPay.Application.Banking;

public sealed record BankGatewayRequest(
    BankOperation Operation,
    decimal Amount,
    string AccountHint,
    Guid CorrelationId);
