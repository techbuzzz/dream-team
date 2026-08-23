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
public sealed class Submission : IAuditableEntity, IHasTenant
{
    public Guid Id { get; private set; } = default!;
    public string TenantId { get; private set; } = default!;
    public Guid ProcessInstanceId { get; private set; }
    public Guid FormVersionId { get; private set; }
    public string AuthorId { get; private set; } = default!;
    public string Data { get; private set; } = default!;
    public bool IsCompensating { get; private set; }
    public Guid? CompensatesSubmissionId { get; private set; }

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    public ProcessInstance? ProcessInstance { get; private set; }

    private Submission() { }

    public static Submission Submit(
        Guid processInstanceId,
        Guid formVersionId,
        string tenantId,
        string authorId,
        string data,
        bool isCompensating = false,
        Guid? compensatesSubmissionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(authorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(data);

        return new Submission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProcessInstanceId = processInstanceId,
            FormVersionId = formVersionId,
            AuthorId = authorId,
            Data = data,
            IsCompensating = isCompensating,
            CompensatesSubmissionId = compensatesSubmissionId,
        };
    }
}
