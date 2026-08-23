using FluentValidation;
using DreamTeam.Modules.Identity.Contracts.v1.Sessions.RevokeSession;

namespace DreamTeam.Modules.Identity.Features.v1.Sessions.RevokeSession;

public sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");
    }
}