using DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionById;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionById;

/// <summary>
/// Non-paginated query — validator added for consistency (400 on
/// Guid.Empty instead of an internal 500 from FirstOrDefaultAsync).
/// </summary>
public sealed class GetSubmissionByIdQueryValidator : AbstractValidator<GetSubmissionByIdQuery>
{
    public GetSubmissionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
