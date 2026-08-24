using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionById;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionById;

/// <summary>
/// Non-paginated queries don't strictly need a validator per the api-conventions
/// rule, but we add one for consistency and to surface a 400 (not a 500 from
/// the underlying FirstOrDefaultAsync) when a client sends Guid.Empty.
/// </summary>
public sealed class GetFormVersionByIdQueryValidator : AbstractValidator<GetFormVersionByIdQuery>
{
    public GetFormVersionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
