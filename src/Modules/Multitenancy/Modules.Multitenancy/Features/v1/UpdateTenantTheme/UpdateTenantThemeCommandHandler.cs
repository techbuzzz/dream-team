using Finbuckle.MultiTenant.Abstractions;
using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Modules.Multitenancy.Contracts.Authorization;
using DreamTeam.Modules.Multitenancy.Contracts;
using DreamTeam.Modules.Multitenancy.Contracts.v1.UpdateTenantTheme;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Features.v1.UpdateTenantTheme;

public sealed class UpdateTenantThemeCommandHandler(
    ITenantThemeService themeService,
    IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor)
    : ICommandHandler<UpdateTenantThemeCommand>
{
    public async ValueTask<Unit> Handle(UpdateTenantThemeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenantId = tenantAccessor.MultiTenantContext?.TenantInfo?.Id
            ?? throw new InvalidOperationException("No tenant context available");

        await themeService.UpdateThemeAsync(tenantId, command.Theme, cancellationToken);

        return Unit.Value;
    }
}