using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionsByInstanceId;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionsByInstanceId;

public sealed class GetSubmissionsByInstanceIdQueryHandler : IQueryHandler<GetSubmissionsByInstanceIdQuery, PagedResponse<SubmissionDto>>
{
    private readonly FormsDbContext _dbContext;

    public GetSubmissionsByInstanceIdQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<SubmissionDto>> Handle(GetSubmissionsByInstanceIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Tenant-isolation filter on Submissions is automatic. The result is
        // an empty page (not 404) for instances the caller can't see — that
        // matches the convention from GetFormVersionsByTemplateId.
        var submissionsQuery = _dbContext.Submissions
            .AsNoTracking()
            .Where(s => s.ProcessInstanceId == query.InstanceId)
            .AsQueryable();

        // Sort allowlist: append-only chains read naturally oldest-first.
        submissionsQuery = query.Sort switch
        {
            "CreatedOnUtcDesc" => submissionsQuery.OrderByDescending(s => s.CreatedOnUtc).ThenBy(s => s.Id),
            "CreatedOnUtc" or null => submissionsQuery.OrderBy(s => s.CreatedOnUtc).ThenBy(s => s.Id),
            _ => submissionsQuery.OrderBy(s => s.CreatedOnUtc).ThenBy(s => s.Id),
        };

        var pageNumber = Math.Max(query.PageNumber ?? 1, 1);
        var pageSize = Math.Clamp(query.PageSize ?? 20, 1, 100);

        var totalCount = await submissionsQuery.LongCountAsync(cancellationToken).ConfigureAwait(false);

        var items = await submissionsQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SubmissionDto(
                s.Id,
                s.ProcessInstanceId,
                s.FormVersionId,
                s.AuthorId,
                s.Data,
                s.IsCompensating,
                s.CompensatesSubmissionId,
                s.CreatedOnUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages == 0) totalPages = 1;

        return new PagedResponse<SubmissionDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }
}
