using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.MarkProcessInstanceAsSkipped;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.MarkProcessInstanceAsSkipped;

public sealed class MarkProcessInstanceAsSkippedCommandHandler : ICommandHandler<MarkProcessInstanceAsSkippedCommand, ProcessInstanceDto>
{
    private readonly FormsDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public MarkProcessInstanceAsSkippedCommandHandler(FormsDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<ProcessInstanceDto> Handle(MarkProcessInstanceAsSkippedCommand command, CancellationToken cancellationToken)
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

        // State-machine guard: terminal instances stay terminal.
        if (instance.Status is Domain.ProcessStatus.Completed or Domain.ProcessStatus.Skipped)
        {
            throw new CustomException(
                $"Process instance '{instance.Id}' is already in terminal state '{instance.Status}'. " +
                "A closed instance cannot be re-skipped (MVP-1 has no reopen flow).",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        // Unlike MarkAsCompleted, we leave CompletedAt null on Skip — the
        // ritual didn't happen, so there's no completion timestamp to record.
        // ExecuteUpdateAsync bypasses the private setters on Status.
        await _dbContext.ProcessInstances
            .Where(i => i.Id == command.InstanceId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(i => i.Status, Domain.ProcessStatus.Skipped)
                    .SetProperty(i => i.CompletedAt, (DateTime?)null),
                cancellationToken)
            .ConfigureAwait(false);

        // Re-read so the returned DTO reflects the post-transition state.
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
