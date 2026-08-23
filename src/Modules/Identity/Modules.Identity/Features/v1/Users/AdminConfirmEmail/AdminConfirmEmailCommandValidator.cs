using FluentValidation;
using DreamTeam.Modules.Identity.Contracts.v1.Users.AdminConfirmEmail;

namespace DreamTeam.Modules.Identity.Features.v1.Users.AdminConfirmEmail;

public sealed class AdminConfirmEmailCommandValidator : AbstractValidator<AdminConfirmEmailCommand>
{
    public AdminConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
