using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.MarkProcessInstanceAsCompleted;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.MarkProcessInstanceAsCompleted;

public sealed class MarkProcessInstanceAsCompletedCommandHandler : ICommandHandler<MarkProcessInstanceAsCompletedCommand, ProcessInstanceDto>
{
    private readonly FormsDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public MarkProcessInstanceAsCompletedCommandHandler(FormsDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<ProcessInstanceDto> Handle(MarkProcessInstanceAsCompletedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenantId = _currentUser.GetTenant();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new CustomException(
                "Request arrived without a resolved tenant. The Forms module requires a tenant context.",
                errors: null,
                System.Net.HttpStatusCode.BadRequest);
        }

        // Load the instance (tenant-isolated). 404 if missing.
        var instance = await _dbContext.ProcessInstances
            .FirstOrDefaultAsync(i => i.Id == command.InstanceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Process instance with ID '{command.InstanceId}' not found.");

        // State-machine guard: only Planned / Running instances can be completed.
        if (instance.Status is Domain.ProcessStatus.Completed or Domain.ProcessStatus.Skipped)
        {
            throw new CustomException(
                $"Process instance '{instance.Id}' is already in terminal state '{instance.Status}'. " +
                "A closed instance cannot be re-completed (MVP-1 has no reopen flow).",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        var completedAt = DateTime.UtcNow;

        // ExecuteUpdateAsync bypasses the entity model's private setters
        // (Status, CompletedAt are { get; private set; }) and writes a
        // single SQL UPDATE. The base DbContext's tenant-isolation filter
        // narrows the WHERE clause to this tenant automatically. We re-load
        // the row to return a fresh DTO with the post-transition state.
        await _dbContext.ProcessInstances
            .Where(i => i.Id == command.InstanceId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(i => i.Status, Domain.ProcessStatus.Completed)
                    .SetProperty(i => i.CompletedAt, (DateTime?)completedAt),
                cancellationToken)
            .ConfigureAwait(false);

        // Refresh the in-memory entity so the returned DTO reflects the
        // new state. (Not strictly required if callers don't read the
        // pre-update values, but cheap and explicit.)
        var updated = await _dbContext.ProcessInstances
            .AsNoTracking()
            .FirstAsync(i => i.Id == command.InstanceId, cancellationToken)
            .ConfigureAwait(false);

        return new ProcessInstanceDto(
            Id: updated.Id,
            FormVersionId: updated.FormVersionId,
            PairUserId: updated.PairUserId,
            ScheduledAt: updated.ScheduledAt,
            Status: updated.Status.ToString(),
            CompletedAt: updated.CompletedAt);
    }
}
