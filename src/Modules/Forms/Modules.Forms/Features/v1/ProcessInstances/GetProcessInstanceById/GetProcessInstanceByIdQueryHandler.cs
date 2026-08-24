using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstanceById;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstanceById;

public sealed class GetProcessInstanceByIdQueryHandler : IQueryHandler<GetProcessInstanceByIdQuery, ProcessInstanceDto>
{
    private readonly FormsDbContext _dbContext;

    public GetProcessInstanceByIdQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<ProcessInstanceDto> Handle(GetProcessInstanceByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // AsNoTracking: read-only projection. Tenant-isolation is automatic.
        var instance = await _dbContext.ProcessInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == query.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Process instance with ID '{query.Id}' not found.");

        return new ProcessInstanceDto(
            Id: instance.Id,
            FormVersionId: instance.FormVersionId,
            PairUserId: instance.PairUserId,
            ScheduledAt: instance.ScheduledAt,
            Status: instance.Status.ToString(),
            CompletedAt: instance.CompletedAt);
    }
}
