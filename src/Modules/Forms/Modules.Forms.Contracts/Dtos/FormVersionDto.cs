namespace DreamTeam.Modules.Forms.Contracts.Dtos;

/// <summary>
/// Read model for a FormVersion (an immutable snapshot of a form's schema).
/// Returned by query commands and by the publish command's response. The
/// <see cref="IsCurrent"/> flag is the renderer / process-instance-generator's
/// pointer to "which version is live right now" — only one FormVersion per
/// ProcessTemplate is current at a time (enforced by a unique partial index
/// in <c>FormVersionConfiguration</c>).
/// </summary>
public sealed record FormVersionDto(
    Guid Id,
    Guid ProcessTemplateId,
    int VersionNumber,
    string? Description,
    bool IsCurrent,
    string PublishedById,
    DateTime PublishedAt);
