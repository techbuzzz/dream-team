using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.UpdateProcessTemplate;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.UpdateProcessTemplate;

public sealed class UpdateProcessTemplateCommandValidator : AbstractValidator<UpdateProcessTemplateCommand>
{
    public UpdateProcessTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        // PATCH with all-null fields is a no-op — reject explicitly with 400
        // rather than silently 200 with no change. This is the validator's
        // job; the handler doesn't need to check.
        RuleFor(x => x)
            .Must(cmd => cmd.Name is not null || cmd.Description is not null || cmd.Category is not null)
            .WithMessage("At least one of Name, Description, or Category must be provided.");

        // When Name is supplied, it must be non-empty (the entity factory
        // requires this too — the validator surfaces it as 400 instead of
        // a 500 from ArgumentException).
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty when provided.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
        });

        When(x => x.Description is not null, () =>
        {
            RuleFor(x => x.Description!)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
        });

        When(x => x.Category is not null, () =>
        {
            RuleFor(x => x.Category!)
                .MaximumLength(64).WithMessage("Category must not exceed 64 characters.");
        });
    }
}
