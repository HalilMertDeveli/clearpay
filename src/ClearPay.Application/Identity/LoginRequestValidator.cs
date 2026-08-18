using ClearPay.Domain.Identity;
using FluentValidation;

namespace ClearPay.Application.Identity;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .When(x => string.IsNullOrWhiteSpace(x.Tc) || !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Geçerli bir e-posta girin.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Tc)
            .Must(tc => DemoTc.ResolveEmail(tc) is not null)
            .WithMessage("Bu demo TC tanımlı değil. Mernis yok.")
            .When(x => string.IsNullOrWhiteSpace(x.Email) && !string.IsNullOrWhiteSpace(x.Tc));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.");
    }
}
