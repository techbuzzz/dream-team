using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.CreateProcessInstance;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.CreateProcessInstance;

public sealed class CreateProcessInstanceCommandValidator : AbstractValidator<CreateProcessInstanceCommand>
{
    /// <summary>
    /// Reject schedules that are too far in the future. A 1-1 scheduled for
    /// the year 2100 is almost certainly a unit-mismatch bug; bounding it
    /// keeps accidental noise out of dashboard queries. 5 years is generous
    /// for any real ritual cadence.
    /// </summary>
    private static readonly DateTime MaxSchedulableAt =
        DateTime.UtcNow.AddYears(5);

    public CreateProcessInstanceCommandValidator()
    {
        RuleFor(x => x.FormVersionId)
            .NotEmpty().WithMessage("FormVersionId is required.");

        // ScheduledAt must be in the future at the moment of scheduling —
        // backdating belongs to an admin flow, not a normal create.
        // We use a small tolerance to absorb clock skew between the client
        // and the server.
        RuleFor(x => x.ScheduledAt)
            .Must(BeInTheFuture)
                .WithMessage("ScheduledAt must be in the future.")
            .Must(BeWithinReasonableHorizon)
                .WithMessage($"ScheduledAt must be no more than 5 years in the future.");

        // PairUserId, when present, is a free-form string (Identity user id).
        // We only bound its length to keep the column healthy.
        RuleFor(x => x.PairUserId)
            .MaximumLength(64)
                .WithMessage("PairUserId must not exceed 64 characters.")
            .When(x => x.PairUserId is not null);
    }

    private static bool BeInTheFuture(DateTime scheduledAt)
    {
        var utc = scheduledAt.Kind switch
        {
            DateTimeKind.Utc => scheduledAt,
            DateTimeKind.Local => scheduledAt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(scheduledAt, DateTimeKind.Utc),
        };
        return utc > DateTime.UtcNow.AddSeconds(-30);   // 30s clock-skew tolerance
    }

    private static bool BeWithinReasonableHorizon(DateTime scheduledAt)
    {
        var utc = scheduledAt.Kind switch
        {
            DateTimeKind.Utc => scheduledAt,
            DateTimeKind.Local => scheduledAt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(scheduledAt, DateTimeKind.Utc),
        };
        return utc <= MaxSchedulableAt;
    }
}
