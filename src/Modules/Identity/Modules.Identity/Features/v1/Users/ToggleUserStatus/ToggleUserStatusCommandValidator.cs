using FluentValidation;
using DreamTeam.Modules.Identity.Contracts.v1.Users.ToggleUserStatus;

namespace DreamTeam.Modules.Identity.Features.v1.Users.ToggleUserStatus;

public sealed class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}