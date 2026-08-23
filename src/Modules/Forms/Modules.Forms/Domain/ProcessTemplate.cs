using DreamTeam.Framework.Core.Domain;

namespace DreamTeam.Modules.Forms.Domain;

/// <summary>
/// A ProcessTemplate is the semantic wrapper for a recurring ritual
/// (e.g. "Weekly 1-1", "Daily Sync", "Sprint Retro"). It owns a chain of
/// <see cref="FormVersion"/>s and is the unit the lead sees in the UI.
/// Per docs/architecture-v1.md §1.
///
/// Audit fields (CreatedOnUtc, CreatedBy, LastModifiedOnUtc,
/// LastModifiedBy) are EF Core SHADOW properties populated by
/// DreamTeam.Framework.Persistence.Inteceptors.AuditableEntitySaveChangesInterceptor;
/// explicit interface implementation here lets the type satisfy the
/// <see cref="IAuditableEntity"/> contract without taking up
/// class-internal slots (the interceptor writes via the property's
/// metadata name, which is the full interface-qualified name).
/// </summary>
public sealed class ProcessTemplate : BaseEntity<Guid>, IAuditableEntity, IHasTenant, ISoftDeletable
{
    public string TenantId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = default!;
    public string? Category { get; set; }          // e.g. "ONE_ON_ONE", "DAILY_SYNC", "RETRO" — see docs/processes.md
    public bool IsArchived { get; set; }

    // ISoftDeletable — IsDeleted is consumed by the BaseDbContext's
    // AppendGlobalQueryFilter("SoftDelete") for tenant-wide hide-not-delete;
    // DeletedOnUtc + DeletedBy are also shadow properties written by the
    // AuditableEntitySaveChangesInterceptor (which is the same interceptor
    // that handles ISoftDeletable).
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }

    // IAuditableEntity — shadow properties, exposed via explicit
    // interface implementation so the C# compiler is satisfied and EF
    // Core still has the property names in the entity metadata.
    DateTimeOffset IAuditableEntity.CreatedOnUtc => default;
    string? IAuditableEntity.CreatedBy => null;
    DateTimeOffset? IAuditableEntity.LastModifiedOnUtc => null;
    string? IAuditableEntity.LastModifiedBy => null;

    // Navigation
    public ICollection<FormVersion> FormVersions { get; set; } = new List<FormVersion>();
}
