using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByUserId;

/// <summary>
/// MVP-1 E1.1 — Paginated list of ProcessInstances where <c>PairUserId</c>
/// equals the supplied userId. Drives the "My 1-1s" dashboard view for
/// a member (their upcoming + past 1-1s with their lead).
///
/// Filter:
/// <list type="bullet">
///   <item><c>UserId</c> — required, the Identity user id to scope by.</item>
/// </list>
///
/// Whole-team rituals (Daily, Retro) have <c>PairUserId = null</c> and do
/// NOT appear in this list — they are surfaced separately via a per-team
/// dashboard (MVP-2).
///
/// Sort allowlist: <c>ScheduledAt</c> (default, soonest first), <c>ScheduledAtDesc</c>,
/// <c>CreatedOnUtc</c>, <c>CreatedOnUtcDesc</c>.
/// </summary>
public sealed record GetProcessInstancesByUserIdQuery(
    string UserId,
    int? PageNumber = 1,
    int? PageSize = 20,
    string? Sort = null)
    : IPagedQuery, IQuery<PagedResponse<ProcessInstanceDto>>
{
    int? IPagedQuery.PageNumber { get; set; } = PageNumber;
    int? IPagedQuery.PageSize { get; set; } = PageSize;
    string? IPagedQuery.Sort { get; set; } = Sort;
}
