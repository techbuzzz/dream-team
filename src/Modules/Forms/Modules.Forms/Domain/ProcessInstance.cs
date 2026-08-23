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
public sealed class ProcessInstance : BaseEntity<Guid>, IAuditableEntity, IHasTenant
{
    public string TenantId { get; set; } = default!;
    public Guid FormVersionId { get; set; }
    public string? PairUserId { get; set; }              // for 1-1; null for whole-team
    public DateTime ScheduledAt { get; set; }
    public ProcessStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }

    // IAuditableEntity — shadow properties.
    DateTimeOffset IAuditableEntity.CreatedOnUtc => default;
    string? IAuditableEntity.CreatedBy => null;
    DateTimeOffset? IAuditableEntity.LastModifiedOnUtc => null;
    string? IAuditableEntity.LastModifiedBy => null;

    public FormVersion? FormVersion { get; set; }
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
