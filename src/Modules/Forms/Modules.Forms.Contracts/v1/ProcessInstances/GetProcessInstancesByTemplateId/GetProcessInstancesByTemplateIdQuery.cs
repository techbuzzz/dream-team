using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByTemplateId;

/// <summary>
/// MVP-1 E1.1 (missed slice) — Paginated list of ProcessInstances that
/// point at any FormVersion of a given ProcessTemplate. EF generates a
/// JOIN through <c>FormVersion.ProcessTemplateId</c> to resolve the
/// indirection (ProcessInstance only stores FormVersionId, not
/// templateId directly).
///
/// Use cases:
/// <list type="bullet">
///   <item>Dashboard "all 1-1s using this template" view.</item>
///   <item>Cleanup: identify instances scheduled against a now-archived version.</item>
/// </list>
///
/// Sort allowlist: <c>ScheduledAt</c> (default, soonest first), <c>ScheduledAtDesc</c>,
/// <c>CreatedOnUtc</c>, <c>CreatedOnUtcDesc</c>.
/// </summary>
public sealed record GetProcessInstancesByTemplateIdQuery(
    Guid TemplateId,
    int? PageNumber = 1,
    int? PageSize = 20,
    string? Sort = null)
    : IPagedQuery, IQuery<PagedResponse<ProcessInstanceDto>>
{
    int? IPagedQuery.PageNumber { get; set; } = PageNumber;
    int? IPagedQuery.PageSize { get; set; } = PageSize;
    string? IPagedQuery.Sort { get; set; } = Sort;
}
