using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.GetProcessTemplateById;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.GetProcessTemplateById;

public sealed class GetProcessTemplateByIdQueryHandler : IQueryHandler<GetProcessTemplateByIdQuery, ProcessTemplateDto>
{
    private readonly FormsDbContext _dbContext;

    public GetProcessTemplateByIdQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<ProcessTemplateDto> Handle(GetProcessTemplateByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The base DbContext's default-on tenant-isolation filter narrows
        // the query to the caller's tenant. If the Id exists in another
        // tenant, this returns null → 404 (which is the right behavior —
        // we never leak that an Id exists outside the caller's tenant).
        var template = await _dbContext.ProcessTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Process template with ID '{query.Id}' not found.");

        return new ProcessTemplateDto(
            Id: template.Id,
            Name: template.Name,
            Slug: template.Slug,
            Description: template.Description,
            OwnerId: template.OwnerId,
            Category: template.Category,
            IsArchived: template.IsArchived,
            CreatedOnUtc: template.CreatedOnUtc);
    }
}
