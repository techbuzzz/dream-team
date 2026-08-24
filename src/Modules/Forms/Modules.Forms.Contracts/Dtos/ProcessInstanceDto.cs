namespace DreamTeam.Modules.Forms.Contracts.Dtos;

/// <summary>
/// Read model for a ProcessInstance (a single occurrence of a ProcessTemplate
/// against a specific FormVersion snapshot). Returned by query commands and
/// by the schedule command's response.
///
/// <see cref="Status"/> is intentionally a string here rather than the
/// Domain <c>ProcessStatus</c> enum so the Contracts project doesn't depend
/// on the internal Domain project. The handler converts via
/// <c>ProcessStatus.ToString()</c>; the API contract stays string-shaped on
/// the wire (matches the host's <c>JsonStringEnumConverter</c> configuration).
/// </summary>
public sealed record ProcessInstanceDto(
    Guid Id,
    Guid FormVersionId,
    string? PairUserId,
    DateTime ScheduledAt,
    string Status,
    DateTime? CompletedAt);
