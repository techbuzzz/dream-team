using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionsByTemplateId;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionsByTemplateId;

public sealed class GetFormVersionsByTemplateIdQueryHandler : IQueryHandler<GetFormVersionsByTemplateIdQuery, PagedResponse<FormVersionDto>>
{
    private readonly FormsDbContext _dbContext;

    public GetFormVersionsByTemplateIdQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<FormVersionDto>> Handle(GetFormVersionsByTemplateIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Tenant-isolation filter on FormVersions is automatic. If the
        // templateId doesn't exist in the caller's tenant, the result is
        // simply an empty page (404 would be ambiguous — we'd leak whether
        // the template exists in another tenant).
        var versionsQuery = _dbContext.FormVersions
            .AsNoTracking()
            .Where(v => v.ProcessTemplateId == query.TemplateId)
            .AsQueryable();

        if (query.IsCurrent.HasValue)
        {
            // Postgres-renderable bool filter; the partial unique index on
            // (TenantId, ProcessTemplateId, IsCurrent=true) means this can
            // return at most one row per template — but the pagination
            // shape stays consistent for the renderer.
            var isCurrent = query.IsCurrent.Value;
            versionsQuery = versionsQuery.Where(v => v.IsCurrent == isCurrent);
        }

        // Sort allowlist: keeps raw user input out of EF's SQL builder.
        versionsQuery = query.Sort switch
        {
            "VersionNumber" or "VersionNumberAsc" or null => versionsQuery.OrderBy(v => v.VersionNumber).ThenBy(v => v.Id),
            "VersionNumberDesc" => versionsQuery.OrderByDescending(v => v.VersionNumber).ThenBy(v => v.Id),
            "PublishedAt" or "PublishedAtAsc" => versionsQuery.OrderBy(v => v.PublishedAt).ThenBy(v => v.Id),
            "PublishedAtDesc" => versionsQuery.OrderByDescending(v => v.PublishedAt).ThenBy(v => v.Id),
            _ => versionsQuery.OrderBy(v => v.VersionNumber).ThenBy(v => v.Id),
        };

        var pageNumber = Math.Max(query.PageNumber ?? 1, 1);
        var pageSize = Math.Clamp(query.PageSize ?? 20, 1, 100);

        var totalCount = await versionsQuery.LongCountAsync(cancellationToken).ConfigureAwait(false);

        var items = await versionsQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new FormVersionDto(
                v.Id,
                v.ProcessTemplateId,
                v.VersionNumber,
                v.Description,
                v.IsCurrent,
                v.PublishedById,
                v.PublishedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages == 0) totalPages = 1;   // an empty result is "page 1 of 1"

        return new PagedResponse<FormVersionDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }
}
