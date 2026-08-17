using ClearPay.Domain.Identity;
using FluentValidation;

namespace ClearPay.Application.Identity;

public sealed class FirebaseTokenRequestValidator : AbstractValidator<FirebaseTokenRequest>
{
    public FirebaseTokenRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Firebase kimlik jetonu zorunludur.");

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone!)
                .Must(TurkishPhone.IsValid).WithMessage("Geçerli bir Türkiye cep numarası girin.");
        });

        RuleFor(x => x.AccountKind)
            .Must(kind => string.IsNullOrWhiteSpace(kind) || AccountKinds.IsKnown(kind))
            .WithMessage("Hesap türü Bireysel veya Kurumsal olmalı.");

        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");
    }
}
