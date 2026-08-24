using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.MarkProcessInstanceAsSkipped;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.MarkProcessInstanceAsSkipped;

public sealed class MarkProcessInstanceAsSkippedCommandValidator : AbstractValidator<MarkProcessInstanceAsSkippedCommand>
{
    public MarkProcessInstanceAsSkippedCommandValidator()
    {
        RuleFor(x => x.InstanceId)
            .NotEmpty().WithMessage("InstanceId is required.");
    }
}
