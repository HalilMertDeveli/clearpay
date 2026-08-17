namespace ClearPay.Application.Funding;

public sealed record LinkedInstrumentDto(
    Guid Id,
    string Last4,
    string Label,
    string AccountHint);
