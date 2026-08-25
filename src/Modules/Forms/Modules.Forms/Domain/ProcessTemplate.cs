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

    /// <summary>
    /// PATCH-style content update. All three fields are optional; null means
    /// "leave unchanged". The caller is responsible for validating the input
    /// (length, non-empty when present) before calling.
    ///
    /// Slug is intentionally NOT updatable here — it is the unique business
    /// key per tenant and changing it would invalidate cached URLs, the
    /// FormVersion chain, and any process instances already pointing at this
    /// template. If a slug rename is ever needed it lands as a separate
    /// dedicated flow (with URL-redirect issuance + instance rewriting).
    ///
    /// OwnerId is also NOT updatable — ownership transfer is a separate
    /// concern (audit, notifications, "transfer to new lead" flow).
    ///
    /// Caller must check IsArchived before calling — the handler refuses
    /// 409 on archived templates.
    /// </summary>
    public void Update(string? name, string? description, string? category)
    {
        if (name is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name;
        }

        if (description is not null)
        {
            // Description can be set to null to clear it. Empty string
            // is rejected by the validator before we get here.
            Description = description;
        }

        if (category is not null)
        {
            // Same as description: null clears, empty rejected upstream.
            Category = category;
        }
    }

    /// <summary>
    /// Soft-archive the template. Archived templates are hidden from the
    /// default list (filter on IsArchived = false) and rejected by the
    /// mutation endpoints with 409. The FormVersion chain stays intact —
    /// historical instances and submissions continue to work and resolve
    /// through their version pointers.
    ///
    /// Idempotency policy: this method throws on the second call. The
    /// handler relies on that to surface 409; a future Unarchive flow
    /// (not in MVP-1) will set IsArchived = false and accept a currently-
    /// archived template.
    /// </summary>
    public void Archive()
    {
        if (IsArchived)
        {
            throw new InvalidOperationException(
                $"Process template '{Id}' is already archived.");
        }
        IsArchived = true;
    }
}
