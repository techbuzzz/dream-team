using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByTemplateId;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByTemplateId;

public sealed class GetProcessInstancesByTemplateIdQueryHandler : IQueryHandler<GetProcessInstancesByTemplateIdQuery, PagedResponse<ProcessInstanceDto>>
{
    private readonly FormsDbContext _dbContext;

    public GetProcessInstancesByTemplateIdQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<ProcessInstanceDto>> Handle(GetProcessInstancesByTemplateIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // ProcessInstance.FormVersionId → FormVersion.ProcessTemplateId.
        // The filter is a JOIN; EF will generate it from the navigation
        // expression. The base DbContext's tenant-isolation filter applies
        // to ProcessInstance rows automatically.
        var instancesQuery = _dbContext.ProcessInstances
            .AsNoTracking()
            .Where(i => i.FormVersion!.ProcessTemplateId == query.TemplateId)
            .AsQueryable();

        instancesQuery = query.Sort switch
        {
            "ScheduledAt" or "ScheduledAtAsc" or null => instancesQuery.OrderBy(i => i.ScheduledAt).ThenBy(i => i.Id),
            "ScheduledAtDesc" => instancesQuery.OrderByDescending(i => i.ScheduledAt).ThenBy(i => i.Id),
            "CreatedOnUtc" or "CreatedOnUtcAsc" => instancesQuery.OrderBy(i => i.CreatedOnUtc).ThenBy(i => i.Id),
            "CreatedOnUtcDesc" => instancesQuery.OrderByDescending(i => i.CreatedOnUtc).ThenBy(i => i.Id),
            _ => instancesQuery.OrderBy(i => i.ScheduledAt).ThenBy(i => i.Id),
        };

        var pageNumber = Math.Max(query.PageNumber ?? 1, 1);
        var pageSize = Math.Clamp(query.PageSize ?? 20, 1, 100);

        var totalCount = await instancesQuery.LongCountAsync(cancellationToken).ConfigureAwait(false);

        var items = await instancesQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ProcessInstanceDto(
                i.Id,
                i.FormVersionId,
                i.PairUserId,
                i.ScheduledAt,
                i.Status.ToString(),
                i.CompletedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages == 0) totalPages = 1;

        return new PagedResponse<ProcessInstanceDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }
}
