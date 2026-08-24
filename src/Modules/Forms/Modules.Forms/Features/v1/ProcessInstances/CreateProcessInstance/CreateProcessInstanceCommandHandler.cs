using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.CreateProcessInstance;
using DreamTeam.Modules.Forms.Data;
using DreamTeam.Modules.Forms.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.CreateProcessInstance;

public sealed class CreateProcessInstanceCommandHandler : ICommandHandler<CreateProcessInstanceCommand, ProcessInstanceDto>
{
    private readonly FormsDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreateProcessInstanceCommandHandler(FormsDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<ProcessInstanceDto> Handle(CreateProcessInstanceCommand command, CancellationToken cancellationToken)
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

        // Tenant-isolation filter applies. The FormVersion must be the
        // current one — refusing historical-version scheduling keeps the
        // MVP-1 flow simple (a lead schedules against the live form).
        var formVersion = await _dbContext.FormVersions
            .FirstOrDefaultAsync(v => v.Id == command.FormVersionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Form version with ID '{command.FormVersionId}' not found.");

        if (!formVersion.IsCurrent)
        {
            // Historical scheduling is an admin / backfill flow that lands later.
            throw new CustomException(
                $"Form version '{formVersion.Id}' is not the current version (versionNumber={formVersion.VersionNumber}). " +
                "MVP-1 only schedules against the current published version.",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        // ScheduledAt is the future timestamp at which the instance is rendered
        // / notified. We treat it as UTC to match the timestamptz column type.
        var scheduledAtUtc = command.ScheduledAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(command.ScheduledAt, DateTimeKind.Utc)
            : command.ScheduledAt.ToUniversalTime();

        var instance = ProcessInstance.Schedule(
            formVersionId: command.FormVersionId,
            tenantId: tenantId,
            scheduledAt: scheduledAtUtc,
            pairUserId: command.PairUserId);

        _dbContext.ProcessInstances.Add(instance);
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
