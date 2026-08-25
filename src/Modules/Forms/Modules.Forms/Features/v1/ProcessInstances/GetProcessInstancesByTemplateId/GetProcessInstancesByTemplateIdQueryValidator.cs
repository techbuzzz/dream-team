using DreamTeam.Framework.Web.Validation;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByTemplateId;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByTemplateId;

public sealed class GetProcessInstancesByTemplateIdQueryValidator : AbstractValidator<GetProcessInstancesByTemplateIdQuery>
{
    public GetProcessInstancesByTemplateIdQueryValidator()
    {
        Include(new PagedQueryValidator<GetProcessInstancesByTemplateIdQuery>());

        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("TemplateId is required.");
    }
}
