using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionsByTemplateId;

/// <summary>
/// MVP-1 E1.1 — Paginated list of FormVersions for a given ProcessTemplate.
/// Used by the Builder to show version history and by the dashboard to
/// compute "published version drift".
///
/// Filters:
/// <list type="bullet">
///   <item><c>TemplateId</c> — required, from the route.</item>
///   <item><c>IsCurrent</c> — optional, restrict to the single live version.</item>
/// </list>
///
/// Sort allowlist (MVP-1, no raw sort strings — see api-conventions.md):
/// <c>VersionNumber</c> (default), <c>VersionNumberDesc</c>, <c>PublishedAt</c>, <c>PublishedAtDesc</c>.
/// </summary>
public sealed record GetFormVersionsByTemplateIdQuery(
    Guid TemplateId,
    bool? IsCurrent = null,
    int? PageNumber = 1,
    int? PageSize = 20,
    string? Sort = null)
    : IPagedQuery, IQuery<PagedResponse<FormVersionDto>>
{
    int? IPagedQuery.PageNumber { get; set; } = PageNumber;
    int? IPagedQuery.PageSize { get; set; } = PageSize;
    string? IPagedQuery.Sort { get; set; } = Sort;
}
