using FluentValidation;

namespace ClearPay.Application.Transfers;

public sealed class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(x => x.ActorUserId).NotEmpty();
        RuleFor(x => x.Recipient)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Amount)
            .GreaterThan(0m)
            .Must(a => decimal.Round(a, 2) == a)
            .WithMessage("Amount must have at most 2 decimal places.");
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(140);
    }
}
