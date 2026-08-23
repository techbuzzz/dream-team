using FluentValidation;
using DreamTeam.Modules.Identity.Contracts.v1.Users.ResendConfirmationEmail;

namespace DreamTeam.Modules.Identity.Features.v1.Users.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
