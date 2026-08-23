using DreamTeam.Framework.Core.Domain;

namespace DreamTeam.Modules.Forms.Domain;

/// <summary>
/// A Submission is an immutable response to a ProcessInstance. Per the
/// FDS append-only invariant: corrections are new rows (with
/// <see cref="IsCompensating"/> + <see cref="CompensatesSubmissionId"/>
/// pointing at the prior submission), never updates.
///
/// Data is a JSONB column — the same shape the FormVersion.Schema
/// describes. AuthorId is a string (Identity user id) so the cross-
/// module FK stays loose (Identity doesn't depend on Forms).
///
/// Audit fields are EF Core shadow properties (see ProcessTemplate).
/// </summary>
public sealed class Submission : BaseEntity<Guid>, IAuditableEntity, IHasTenant
{
    public string TenantId { get; set; } = default!;
    public Guid ProcessInstanceId { get; set; }
    public Guid FormVersionId { get; set; }            // denormalised snapshot of the version that was active
    public string AuthorId { get; set; } = default!;
    public string Data { get; set; } = default!;       // jsonb — answers keyed by element id
    public bool IsCompensating { get; set; }
    public Guid? CompensatesSubmissionId { get; set; }

    // IAuditableEntity — shadow properties.
    DateTimeOffset IAuditableEntity.CreatedOnUtc => default;
    string? IAuditableEntity.CreatedBy => null;
    DateTimeOffset? IAuditableEntity.LastModifiedOnUtc => null;
    string? IAuditableEntity.LastModifiedBy => null;

    public ProcessInstance? ProcessInstance { get; set; }
}
