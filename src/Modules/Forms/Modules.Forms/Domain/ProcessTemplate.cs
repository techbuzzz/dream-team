using DreamTeam.Framework.Core.Domain;

namespace DreamTeam.Modules.Forms.Domain;

/// <summary>
/// A ProcessTemplate is the semantic wrapper for a recurring ritual
/// (e.g. "Weekly 1-1", "Daily Sync", "Sprint Retro"). It owns a chain of
/// <see cref="FormVersion"/>s and is the unit the lead sees in the UI.
/// Per docs/architecture-v1.md §1.
///
/// Audit fields (CreatedOnUtc, CreatedBy, LastModifiedOnUtc,
/// LastModifiedBy) are written by
/// DreamTeam.Framework.Persistence.Inteceptors.AuditableEntitySaveChangesInterceptor
/// before SaveChanges — they're real CLR properties here (not shadow
/// properties) so other code can read them without metadata lookups.
/// </summary>
public sealed class ProcessTemplate : IAuditableEntity, IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; } = default!;
    public string TenantId { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public string OwnerId { get; private set; } = default!;
    public string? Category { get; private set; }
    public bool IsArchived { get; private set; }

    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    // IAuditableEntity — public properties with private set; the
    // AuditableEntitySaveChangesInterceptor writes to these via
    // entry.Property(...). The framework reads them when building
    // the permission catalog and audit views.
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // Navigation
    public ICollection<FormVersion> FormVersions { get; private set; } = new List<FormVersion>();

    // EF Core
    private ProcessTemplate() { }

    public static ProcessTemplate Create(
        string tenantId,
        string name,
        string slug,
        string? description,
        string ownerId,
        string? category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        return new ProcessTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Slug = slug,
            Description = description,
            OwnerId = ownerId,
            Category = category,
            IsArchived = false,
            IsDeleted = false,
        };
    }
}
