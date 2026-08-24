using DreamTeam.Framework.Web.Validation;
using DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionsByInstanceId;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionsByInstanceId;

public sealed class GetSubmissionsByInstanceIdQueryValidator : AbstractValidator<GetSubmissionsByInstanceIdQuery>
{
    public GetSubmissionsByInstanceIdQueryValidator()
    {
        // PagedQueryValidator enforces PageNumber >= 1, PageSize 1-100, Sort length.
        Include(new PagedQueryValidator<GetSubmissionsByInstanceIdQuery>());

        RuleFor(x => x.InstanceId)
            .NotEmpty().WithMessage("InstanceId is required.");
    }
}
