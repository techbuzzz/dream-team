using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.GetProcessTemplates;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.GetProcessTemplates;

public sealed class GetProcessTemplatesQueryHandler : IQueryHandler<GetProcessTemplatesQuery, PagedResponse<ProcessTemplateDto>>
{
    private readonly FormsDbContext _dbContext;

    public GetProcessTemplatesQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<ProcessTemplateDto>> Handle(GetProcessTemplatesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Soft-deleted templates are excluded by the base DbContext's
        // AppendGlobalQueryFilter("SoftDelete").
        var templatesQuery = _dbContext.ProcessTemplates
            .AsNoTracking()
            .AsQueryable();

        // Apply search filter (case-insensitive on Name + Slug).
        // EF.Functions.ILike is the Postgres-friendly case-insensitive LIKE
        // (uses a btree index on the lowercased expression if one exists).
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var pattern = $"%{query.SearchTerm.Trim()}%";
            templatesQuery = templatesQuery.Where(t =>
                EF.Functions.ILike(t.Name, pattern) ||
                EF.Functions.ILike(t.Slug, pattern));
        }

        // Sort: support a small allowlist ("Name", "NameAsc", "NameDesc",
        // "CreatedOnUtc", "CreatedOnUtcAsc", "CreatedOnUtcDesc") to avoid
        // exposing the raw OrderBy string to EF's SQL builder. MVP-1
        // doesn't need a full Sort expression parser.
        templatesQuery = query.Sort switch
        {
            "Name" or "NameAsc" or null => templatesQuery.OrderBy(t => t.Name).ThenBy(t => t.Id),
            "NameDesc" => templatesQuery.OrderByDescending(t => t.Name).ThenBy(t => t.Id),
            "CreatedOnUtc" or "CreatedOnUtcAsc" => templatesQuery.OrderBy(t => t.CreatedOnUtc).ThenBy(t => t.Id),
            "CreatedOnUtcDesc" => templatesQuery.OrderByDescending(t => t.CreatedOnUtc).ThenBy(t => t.Id),
            _ => templatesQuery.OrderBy(t => t.Name).ThenBy(t => t.Id),
        };

        var pageNumber = Math.Max(query.PageNumber ?? 1, 1);
        var pageSize = Math.Clamp(query.PageSize ?? 20, 1, 100);

        var totalCount = await templatesQuery.LongCountAsync(cancellationToken).ConfigureAwait(false);

        var items = await templatesQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new ProcessTemplateDto(
                t.Id,
                t.Name,
                t.Slug,
                t.Description,
                t.OwnerId,
                t.Category,
                t.IsArchived,
                t.CreatedOnUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages == 0) totalPages = 1;   // an empty result is "page 1 of 1"

        return new PagedResponse<ProcessTemplateDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }
}
