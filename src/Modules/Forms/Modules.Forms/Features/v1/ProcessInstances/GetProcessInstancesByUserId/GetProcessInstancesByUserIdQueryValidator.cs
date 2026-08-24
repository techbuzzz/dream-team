using DreamTeam.Framework.Web.Validation;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByUserId;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByUserId;

public sealed class GetProcessInstancesByUserIdQueryValidator : AbstractValidator<GetProcessInstancesByUserIdQuery>
{
    public GetProcessInstancesByUserIdQueryValidator()
    {
        // PagedQueryValidator enforces PageNumber >= 1, PageSize 1-100, Sort length.
        Include(new PagedQueryValidator<GetProcessInstancesByUserIdQuery>());

        // UserId comes from the query string; reject empty / whitespace
        // with a 400 instead of "no rows".
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .MaximumLength(64).WithMessage("UserId must not exceed 64 characters.");
    }
}
