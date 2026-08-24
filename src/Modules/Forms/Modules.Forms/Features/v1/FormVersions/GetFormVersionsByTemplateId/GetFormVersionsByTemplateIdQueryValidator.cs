using DreamTeam.Framework.Web.Validation;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionsByTemplateId;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionsByTemplateId;

public sealed class GetFormVersionsByTemplateIdQueryValidator : AbstractValidator<GetFormVersionsByTemplateIdQuery>
{
    public GetFormVersionsByTemplateIdQueryValidator()
    {
        // PagedQueryValidator enforces PageNumber >= 1, PageSize 1-100, Sort length.
        Include(new PagedQueryValidator<GetFormVersionsByTemplateIdQuery>());

        // TemplateId comes from the route; rejecting Guid.Empty here gives
        // a clean 400 instead of a "no rows" response shape.
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("TemplateId is required.");
    }
}
