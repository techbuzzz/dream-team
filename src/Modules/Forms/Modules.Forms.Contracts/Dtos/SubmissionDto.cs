namespace DreamTeam.Modules.Forms.Contracts.Dtos;

/// <summary>
/// Read model for a Submission (an immutable, append-only response to a
/// ProcessInstance). Returned by query commands and by the submit command's
/// response.
///
/// Per Golden Rule #3 in AGENTS.md (append-only): corrections are new rows,
/// not updates. <see cref="IsCompensating"/> + <see cref="CompensatesSubmissionId"/>
/// record the "this row amends that row" link, but the original submission
/// is never mutated or deleted.
/// </summary>
public sealed record SubmissionDto(
    Guid Id,
    Guid ProcessInstanceId,
    Guid FormVersionId,
    string AuthorId,
    string Data,
    bool IsCompensating,
    Guid? CompensatesSubmissionId,
    DateTimeOffset CreatedOnUtc);
