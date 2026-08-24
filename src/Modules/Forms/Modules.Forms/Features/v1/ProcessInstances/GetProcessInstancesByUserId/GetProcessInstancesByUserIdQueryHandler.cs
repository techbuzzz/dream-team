using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByUserId;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByUserId;

public sealed class GetProcessInstancesByUserIdQueryHandler : IQueryHandler<GetProcessInstancesByUserIdQuery, PagedResponse<ProcessInstanceDto>>
{
    private readonly FormsDbContext _dbContext;

    public GetProcessInstancesByUserIdQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<ProcessInstanceDto>> Handle(GetProcessInstancesByUserIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Tenant-isolation filter is automatic. The result is the caller's
        // tenant's instances for this user — empty if the user has no
        // 1-1s scheduled in this tenant.
        var instancesQuery = _dbContext.ProcessInstances
            .AsNoTracking()
            .Where(i => i.PairUserId == query.UserId)
            .AsQueryable();

        // Sort allowlist. Soonest-first is the natural dashboard order
        // ("what's my next 1-1?").
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
