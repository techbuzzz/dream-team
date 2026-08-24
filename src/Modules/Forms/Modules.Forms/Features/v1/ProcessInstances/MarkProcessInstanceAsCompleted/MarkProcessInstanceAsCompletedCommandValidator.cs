using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.MarkProcessInstanceAsCompleted;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.MarkProcessInstanceAsCompleted;

public sealed class MarkProcessInstanceAsCompletedCommandValidator : AbstractValidator<MarkProcessInstanceAsCompletedCommand>
{
    public MarkProcessInstanceAsCompletedCommandValidator()
    {
        RuleFor(x => x.InstanceId)
            .NotEmpty().WithMessage("InstanceId is required.");
    }
}
