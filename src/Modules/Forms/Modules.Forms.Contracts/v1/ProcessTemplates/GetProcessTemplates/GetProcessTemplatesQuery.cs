using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.GetProcessTemplates;

/// <summary>
/// MVP-1 E1.4 (Renderer list view) prerequisite: paginated list of
/// process templates. Optional <see cref="SearchTerm"/> matches
/// against Name and Slug (case-insensitive). Soft-deleted templates
/// are excluded by the base DbContext's query filter.
/// </summary>
public sealed record GetProcessTemplatesQuery(
    int? PageNumber = 1,
    int? PageSize = 20,
    string? Sort = null,
    string? SearchTerm = null)
    : IPagedQuery, IQuery<PagedResponse<ProcessTemplateDto>>
{
    int? IPagedQuery.PageNumber { get; set; } = PageNumber;
    int? IPagedQuery.PageSize { get; set; } = PageSize;
    string? IPagedQuery.Sort { get; set; } = Sort;
}
