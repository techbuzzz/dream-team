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
/// AuditableEntitySaveChangesInterceptor).
/// </summary>
public sealed class FormVersion : IAuditableEntity, IHasTenant
{
    public Guid Id { get; private set; } = default!;
    public string TenantId { get; private set; } = default!;
    public Guid ProcessTemplateId { get; private set; }
    public int VersionNumber { get; private set; }
    public string Schema { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsCurrent { get; private set; }
    public string PublishedById { get; private set; } = default!;
    public DateTime PublishedAt { get; private set; }

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    public ProcessTemplate? ProcessTemplate { get; private set; }
    public ICollection<ProcessInstance> ProcessInstances { get; private set; } = new List<ProcessInstance>();

    private FormVersion() { }

    public static FormVersion Publish(
        Guid processTemplateId,
        string tenantId,
        int versionNumber,
        string schema,
        string? description,
        string publishedById)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);
        ArgumentException.ThrowIfNullOrWhiteSpace(publishedById);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(versionNumber);

        return new FormVersion
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProcessTemplateId = processTemplateId,
            VersionNumber = versionNumber,
            Schema = schema,
            Description = description,
            IsCurrent = true,
            PublishedById = publishedById,
            PublishedAt = DateTime.UtcNow,
        };
    }
}
