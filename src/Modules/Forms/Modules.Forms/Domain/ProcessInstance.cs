using DreamTeam.Framework.Core.Domain;

namespace DreamTeam.Modules.Forms.Domain;

public enum ProcessStatus
{
    Planned = 0,
    Running = 1,
    Completed = 2,
    Skipped = 3,
}

/// <summary>
/// A ProcessInstance is a single occurrence of a template — e.g. "the
/// 1-1 for Alice on 2026-09-01". It points at ONE FormVersion (snapshot)
/// regardless of how the template has evolved since.
///
/// MVP-1 is single-tenant; TeamId is the AppTenantInfo.Id. The 1:many
/// "pair" pattern (1-1 is between a lead and a member) is captured by
/// the PairUserId field. PairUserId is null for whole-team rituals
/// (Daily, Retro).
///
/// Audit fields are EF Core shadow properties (see ProcessTemplate).
/// </summary>
public sealed class ProcessInstance : IAuditableEntity, IHasTenant
{
    public Guid Id { get; private set; } = default!;
    public string TenantId { get; private set; } = default!;
    public Guid FormVersionId { get; private set; }
    public string? PairUserId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public ProcessStatus Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    public FormVersion? FormVersion { get; private set; }
    public ICollection<Submission> Submissions { get; private set; } = new List<Submission>();

    private ProcessInstance() { }

    public static ProcessInstance Schedule(
        Guid formVersionId,
        string tenantId,
        DateTime scheduledAt,
        string? pairUserId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        return new ProcessInstance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FormVersionId = formVersionId,
            PairUserId = pairUserId,
            ScheduledAt = scheduledAt,
            Status = ProcessStatus.Planned,
            CompletedAt = null,
        };
    }
}
