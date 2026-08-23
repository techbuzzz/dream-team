using DreamTeam.Framework.Core.Domain;

namespace DreamTeam.Modules.Forms.Domain;

/// <summary>
/// A FormVersion is an IMMUTABLE snapshot of a form's schema. Once
/// published, the JSON is fixed — every <see cref="ProcessInstance"/>
/// that points at this version renders the same form.
///
/// Snapshot-on-publish (per docs/architecture-v1.md §1): renaming or
/// editing a template never mutates past instances.
///
/// Schema is a JSONB column. The shape is the Form DSL described in
/// docs/architecture-v1.md §2 — a tree of pages, each with a list of
/// elements (rating, longtext, skill_wheel, …). Server-side validation
/// mirrors the Zod schema the renderer uses.
///
/// Audit fields are EF Core shadow properties (see
/// AuditableEntitySaveChangesInterceptor). TenantId is a real
/// property because Finbuckle's tenant filter uses the column
/// directly.
/// </summary>
public sealed class FormVersion : BaseEntity<Guid>, IAuditableEntity, IHasTenant
{
    public string TenantId { get; set; } = default!;
    public Guid ProcessTemplateId { get; set; }
    public int VersionNumber { get; set; }        // monotonic per template
    public string Schema { get; set; } = default!; // jsonb — Form DSL
    public string? Description { get; set; }       // changelog-style
    public bool IsCurrent { get; set; }            // only one IsCurrent=true per template
    public string PublishedById { get; set; } = default!;
    public DateTime PublishedAt { get; set; }

    // IAuditableEntity — shadow properties (see ProcessTemplate for rationale).
    DateTimeOffset IAuditableEntity.CreatedOnUtc => default;
    string? IAuditableEntity.CreatedBy => null;
    DateTimeOffset? IAuditableEntity.LastModifiedOnUtc => null;
    string? IAuditableEntity.LastModifiedBy => null;

    public ProcessTemplate? ProcessTemplate { get; set; }
    public ICollection<ProcessInstance> ProcessInstances { get; set; } = new List<ProcessInstance>();
}
