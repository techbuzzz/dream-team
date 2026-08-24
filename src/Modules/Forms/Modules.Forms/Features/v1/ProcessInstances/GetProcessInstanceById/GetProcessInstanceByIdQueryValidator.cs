using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstanceById;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstanceById;

/// <summary>
/// Non-paginated queries don't strictly need a validator per the api-conventions
/// rule, but we add one for consistency and to surface a 400 (not a 500 from
/// the underlying FirstOrDefaultAsync) when a client sends Guid.Empty.
/// </summary>
public sealed class GetProcessInstanceByIdQueryValidator : AbstractValidator<GetProcessInstanceByIdQuery>
{
    public GetProcessInstanceByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
