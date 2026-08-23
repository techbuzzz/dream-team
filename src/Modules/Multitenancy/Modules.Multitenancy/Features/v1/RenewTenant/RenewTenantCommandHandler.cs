using DreamTeam.Modules.Multitenancy.Contracts;
using DreamTeam.Modules.Multitenancy.Contracts.v1.RenewTenant;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Features.v1.RenewTenant;

public sealed class RenewTenantCommandHandler(
    ITenantService tenantService)
    : ICommandHandler<RenewTenantCommand, RenewTenantCommandResponse>
{
    // MVP-1: default renewal extends validity by 12 months. The FDS calls for a
    // billing-driven renewal; that integration is removed with the FSH Billing
    // module and will be reintroduced in v4.
    private const int DefaultTermMonths = 12;
    private const string DefaultPlanKey = "default";

    public async ValueTask<RenewTenantCommandResponse> Handle(RenewTenantCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var (_, validUpto, planChanged) = await tenantService
            .RenewAsync(command.TenantId, DefaultPlanKey, DefaultTermMonths, cancellationToken)
            .ConfigureAwait(false);

        return new RenewTenantCommandResponse(command.TenantId, validUpto, DefaultPlanKey, planChanged);
    }
}
