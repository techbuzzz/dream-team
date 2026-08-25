using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.ArchiveProcessTemplate;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.ArchiveProcessTemplate;

public sealed class ArchiveProcessTemplateCommandValidator : AbstractValidator<ArchiveProcessTemplateCommand>
{
    public ArchiveProcessTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
