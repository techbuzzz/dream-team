using FluentValidation;
using DreamTeam.Modules.Identity.Contracts.v1.TwoFactor;

namespace DreamTeam.Modules.Identity.Features.v1.TwoFactor.Disable;

public sealed class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
    }
}
