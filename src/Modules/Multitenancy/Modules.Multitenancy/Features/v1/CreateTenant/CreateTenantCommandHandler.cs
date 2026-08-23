using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Modules.Multitenancy.Contracts;
using DreamTeam.Modules.Multitenancy.Contracts.v1.CreateTenant;
using DreamTeam.Modules.Multitenancy.Provisioning;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Features.v1.CreateTenant;

public sealed class CreateTenantCommandHandler(
    ITenantService tenantService,
    ITenantProvisioningService provisioningService,
    ITenantInitialPasswordBuffer passwordBuffer,
    TimeProvider timeProvider)
    : ICommandHandler<CreateTenantCommand, CreateTenantCommandResponse>
{
    // MVP-1: default tenant validity is 12 months from creation. The FDS calls for
    // a billing integration to set the term via the Billing module's plan; that
    // integration is removed with the FSH Billing module and will be reintroduced
    // in v4 (or earlier if a DreamTeam billing surface is added).
    private const int DefaultTermMonths = 12;
    private const string DefaultPlanKey = "default";

    public async ValueTask<CreateTenantCommandResponse> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var periodStart = timeProvider.GetUtcNow().UtcDateTime;
        var periodEnd = periodStart.AddMonths(DefaultTermMonths);

        var tenantId = await tenantService.CreateAsync(
            command.Id,
            command.Name,
            command.ConnectionString,
            command.AdminEmail,
            command.Issuer,
            DefaultPlanKey,
            periodEnd,
            cancellationToken).ConfigureAwait(false);

        // Buffer the admin password for IdentityDbInitializer's background seed step,
        // storing it before StartAsync so the seed never runs ahead of the buffer.
        passwordBuffer.Store(tenantId, command.AdminPassword);

        var provisioning = await provisioningService.StartAsync(tenantId, cancellationToken).ConfigureAwait(false);

        return new CreateTenantCommandResponse(
            tenantId,
            provisioning.CorrelationId,
            provisioning.Status.ToString());
    }
}
