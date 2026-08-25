using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetCurrentFormVersion;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.GetCurrentFormVersion;

public sealed class GetCurrentFormVersionQueryHandler : IQueryHandler<GetCurrentFormVersionQuery, FormVersionDto>
{
    private readonly FormsDbContext _dbContext;

    public GetCurrentFormVersionQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<FormVersionDto> Handle(GetCurrentFormVersionQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Single-row lookup. The partial unique index
        //   IX_FormVersions_(TenantId, ProcessTemplateId, IsCurrent=true) WHERE IsCurrent = true
        // makes this a single-row seek under the hood — no need to scan + filter.
        var version = await _dbContext.FormVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                v => v.ProcessTemplateId == query.TemplateId && v.IsCurrent,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(
                $"No current FormVersion for template '{query.TemplateId}'. " +
                "Either the template does not exist or no FormVersion has been published yet.");

        return new FormVersionDto(
            Id: version.Id,
            ProcessTemplateId: version.ProcessTemplateId,
            VersionNumber: version.VersionNumber,
            Description: version.Description,
            IsCurrent: version.IsCurrent,
            PublishedById: version.PublishedById,
            PublishedAt: version.PublishedAt);
    }
}
