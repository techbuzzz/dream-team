using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.CreateProcessTemplate;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.CreateProcessTemplate;

public sealed class CreateProcessTemplateCommandValidator : AbstractValidator<CreateProcessTemplateCommand>
{
    public CreateProcessTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        // Slug: kebab-case ASCII enforced as a soft pattern check (the
        // database index makes the final call). The regex matches
        // 1-100 chars of [a-z0-9-] without leading/trailing dashes and
        // without consecutive dashes — see docs/processes.md for the
        // canonical slug shape.
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters.")
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug must be kebab-case (lowercase, digits, single dashes, no leading/trailing dash).");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(64).WithMessage("Category must not exceed 64 characters.");
    }
}
