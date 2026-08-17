using ClearPay.Domain.Identity;
using FluentValidation;

namespace ClearPay.Application.Identity;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        When(x => string.IsNullOrWhiteSpace(x.Tc), () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir e-posta girin.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Tc), () =>
        {
            RuleFor(x => x.Tc)
                .Must(tc => DemoTc.ResolveEmail(tc) is not null)
                .WithMessage("Bu demo TC tanımlı değil. Mernis yok.");
        });

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.");
    }
}
