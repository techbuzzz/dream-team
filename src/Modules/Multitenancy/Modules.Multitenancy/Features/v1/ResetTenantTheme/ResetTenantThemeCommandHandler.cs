using Finbuckle.MultiTenant.Abstractions;
using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Modules.Multitenancy.Contracts.Authorization;
using DreamTeam.Modules.Multitenancy.Contracts;
using DreamTeam.Modules.Multitenancy.Contracts.v1.ResetTenantTheme;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Features.v1.ResetTenantTheme;

public sealed class ResetTenantThemeCommandHandler(
    ITenantThemeService themeService,
    IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor)
    : ICommandHandler<ResetTenantThemeCommand>
{
    public async ValueTask<Unit> Handle(ResetTenantThemeCommand command, CancellationToken cancellationToken)
    {
        var tenantId = tenantAccessor.MultiTenantContext?.TenantInfo?.Id
            ?? throw new InvalidOperationException("No tenant context available");

        await themeService.ResetThemeAsync(tenantId, cancellationToken);

        return Unit.Value;
    }
}