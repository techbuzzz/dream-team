using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionById;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionById;

public sealed class GetFormVersionByIdQueryHandler : IQueryHandler<GetFormVersionByIdQuery, FormVersionDto>
{
    private readonly FormsDbContext _dbContext;

    public GetFormVersionByIdQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<FormVersionDto> Handle(GetFormVersionByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // AsNoTracking: read-only projection. The tenant-isolation query filter
        // on BaseDbContext narrows the WHERE to the caller's tenant automatically.
        var version = await _dbContext.FormVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == query.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Form version with ID '{query.Id}' not found.");

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
