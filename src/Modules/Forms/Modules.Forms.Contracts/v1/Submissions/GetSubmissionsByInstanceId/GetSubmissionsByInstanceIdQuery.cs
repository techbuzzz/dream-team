using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionsByInstanceId;

/// <summary>
/// MVP-1 E1.1 — Paginated list of Submissions for a given ProcessInstance.
/// Returns ALL submissions — original + compensating corrections — in
/// CreatedOnUtc ascending order (oldest first; the append-only chain reads
/// naturally this way). Callers that need just the "current" answer
/// filter client-side by walking the IsCompensating links.
///
/// Sort allowlist: <c>CreatedOnUtc</c> (default), <c>CreatedOnUtcDesc</c>.
/// Other sorts (e.g. by AuthorId) are not exposed in MVP-1 — too easy to
/// accidentally cross-tenant leak if a UI builds a query that EF doesn't
/// translate cleanly.
/// </summary>
public sealed record GetSubmissionsByInstanceIdQuery(
    Guid InstanceId,
    int? PageNumber = 1,
    int? PageSize = 20,
    string? Sort = null)
    : IPagedQuery, IQuery<PagedResponse<SubmissionDto>>
{
    int? IPagedQuery.PageNumber { get; set; } = PageNumber;
    int? IPagedQuery.PageSize { get; set; } = PageSize;
    string? IPagedQuery.Sort { get; set; } = Sort;
}
