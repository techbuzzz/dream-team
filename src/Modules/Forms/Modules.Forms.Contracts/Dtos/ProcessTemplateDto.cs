namespace DreamTeam.Modules.Forms.Contracts.Dtos;

/// <summary>
/// Read model for a process template (the semantic wrapper for a recurring
/// ritual). Returned by query commands and by the create command's response.
/// The Slug is what callers use to construct a URL.
/// </summary>
public sealed record ProcessTemplateDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string OwnerId,
    string? Category,
    bool IsArchived,
    DateTimeOffset CreatedOnUtc);
