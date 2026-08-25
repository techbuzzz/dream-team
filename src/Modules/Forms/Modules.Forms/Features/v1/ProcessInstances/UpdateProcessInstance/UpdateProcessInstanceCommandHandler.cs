using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.UpdateProcessInstance;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.UpdateProcessInstance;

public sealed class UpdateProcessInstanceCommandHandler : ICommandHandler<UpdateProcessInstanceCommand, ProcessInstanceDto>
{
    private readonly FormsDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public UpdateProcessInstanceCommandHandler(FormsDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<ProcessInstanceDto> Handle(UpdateProcessInstanceCommand command, CancellationToken cancellationToken)
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

        // Load tracked (we're about to mutate). Tenant filter narrows WHERE.
        var instance = await _dbContext.ProcessInstances
            .FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Process instance with ID '{command.Id}' not found.");

        // Terminal instances are immutable — there's no MVP-1 "reopen" flow.
        if (instance.Status is Domain.ProcessStatus.Completed or Domain.ProcessStatus.Skipped)
        {
            throw new CustomException(
                $"Process instance '{instance.Id}' is in terminal state '{instance.Status}'. " +
                "Closed instances cannot be rescheduled or reassigned.",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        // Domain method enforces the per-field guards (non-empty when present,
        // null = leave unchanged). FormVersionId is not touchable.
        instance.Update(
            scheduledAt: command.ScheduledAt,
            pairUserId: command.PairUserId);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ProcessInstanceDto(
            Id: instance.Id,
            FormVersionId: instance.FormVersionId,
            PairUserId: instance.PairUserId,
            ScheduledAt: instance.ScheduledAt,
            Status: instance.Status.ToString(),
            CompletedAt: instance.CompletedAt);
    }
}
