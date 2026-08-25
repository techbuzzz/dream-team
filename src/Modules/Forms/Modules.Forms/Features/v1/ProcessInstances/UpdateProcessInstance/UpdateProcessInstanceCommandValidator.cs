using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.UpdateProcessInstance;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.UpdateProcessInstance;

public sealed class UpdateProcessInstanceCommandValidator : AbstractValidator<UpdateProcessInstanceCommand>
{
    public UpdateProcessInstanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        // PATCH with all-null fields is a no-op — reject with 400 so the
        // caller knows their request had no effect.
        RuleFor(x => x)
            .Must(cmd => cmd.ScheduledAt.HasValue || cmd.PairUserId is not null)
            .WithMessage("At least one of ScheduledAt or PairUserId must be provided.");

        // When ScheduledAt is supplied, it must be in the future with a
        // small clock-skew tolerance (mirrors CreateProcessInstance).
        When(x => x.ScheduledAt.HasValue, () =>
        {
            RuleFor(x => x.ScheduledAt!.Value)
                .Must(BeInTheFuture)
                    .WithMessage("ScheduledAt must be in the future.")
                .Must(BeWithinReasonableHorizon)
                    .WithMessage("ScheduledAt must be no more than 5 years in the future.");
        });

        // When PairUserId is supplied, it must be non-empty.
        When(x => x.PairUserId is not null, () =>
        {
            RuleFor(x => x.PairUserId!)
                .NotEmpty().WithMessage("PairUserId cannot be empty when provided.")
                .MaximumLength(64).WithMessage("PairUserId must not exceed 64 characters.");
        });
    }

    private static bool BeInTheFuture(DateTime scheduledAt)
    {
        var utc = scheduledAt.Kind switch
        {
            DateTimeKind.Utc => scheduledAt,
            DateTimeKind.Local => scheduledAt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(scheduledAt, DateTimeKind.Utc),
        };
        return utc > DateTime.UtcNow.AddSeconds(-30);
    }

    private static bool BeWithinReasonableHorizon(DateTime scheduledAt)
    {
        var utc = scheduledAt.Kind switch
        {
            DateTimeKind.Utc => scheduledAt,
            DateTimeKind.Local => scheduledAt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(scheduledAt, DateTimeKind.Utc),
        };
        return utc <= DateTime.UtcNow.AddYears(5);
    }
}
