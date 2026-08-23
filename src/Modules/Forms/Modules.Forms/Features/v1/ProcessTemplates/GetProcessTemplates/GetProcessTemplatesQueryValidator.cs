using DreamTeam.Framework.Web.Validation;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.GetProcessTemplates;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.GetProcessTemplates;

public sealed class GetProcessTemplatesQueryValidator : AbstractValidator<GetProcessTemplatesQuery>
{
    public GetProcessTemplatesQueryValidator()
    {
        // PagedQueryValidator enforces PageNumber >= 1, PageSize 1-100, Sort length.
        // Forms.Tests verifies the values that matter for the Forms module.
        Include(new PagedQueryValidator<GetProcessTemplatesQuery>());

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("Search term must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}
