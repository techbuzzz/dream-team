using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetCurrentFormVersion;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.GetCurrentFormVersion;

/// <summary>
/// Non-paginated query — validator is added for consistency (400 on
/// empty TemplateId instead of an internal 500 from FirstOrDefaultAsync).
/// </summary>
public sealed class GetCurrentFormVersionQueryValidator : AbstractValidator<GetCurrentFormVersionQuery>
{
    public GetCurrentFormVersionQueryValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("TemplateId is required.");
    }
}
