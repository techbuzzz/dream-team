using Finbuckle.MultiTenant.Abstractions;
using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Modules.Multitenancy.Contracts.Authorization;
using DreamTeam.Modules.Multitenancy.Contracts.Dtos;
using DreamTeam.Modules.Multitenancy.Contracts.v1.GetTenantMigrations;
using DreamTeam.Modules.Multitenancy.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DreamTeam.Modules.Multitenancy.Features.v1.GetTenantMigrations;

public sealed class GetTenantMigrationsQueryHandler
    : IQueryHandler<GetTenantMigrationsQuery, IReadOnlyCollection<TenantMigrationStatusDto>>
{
    private readonly IMultiTenantStore<AppTenantInfo> _tenantStore;
    private readonly IServiceScopeFactory _scopeFactory;

    public GetTenantMigrationsQueryHandler(
        IMultiTenantStore<AppTenantInfo> tenantStore,
        IServiceScopeFactory scopeFactory)
    {
        _tenantStore = tenantStore;
        _scopeFactory = scopeFactory;
    }

    public async ValueTask<IReadOnlyCollection<TenantMigrationStatusDto>> Handle(
        GetTenantMigrationsQuery query,
        CancellationToken cancellationToken)
    {
        var tenants = await _tenantStore.GetAllAsync().ConfigureAwait(false);

        var tenantMigrationStatuses = new List<TenantMigrationStatusDto>();

        foreach (var tenant in tenants)
        {
            var tenantStatus = new TenantMigrationStatusDto
            {
                TenantId = tenant.Id,
                Name = tenant.Name!,
                IsActive = tenant.IsActive,
                ValidUpto = tenant.ValidUpto
            };

            try
            {
                using IServiceScope tenantScope = _scopeFactory.CreateScope();

                tenantScope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()
                    .MultiTenantContext = new MultiTenantContext<AppTenantInfo>(tenant);

                var dbContext = tenantScope.ServiceProvider.GetRequiredService<TenantDbContext>();

                var appliedMigrations = await dbContext.Database
                    .GetAppliedMigrationsAsync(cancellationToken)
                    .ConfigureAwait(false);

                var pendingMigrations = await dbContext.Database
                    .GetPendingMigrationsAsync(cancellationToken)
                    .ConfigureAwait(false);

                tenantStatus.Provider = dbContext.Database.ProviderName;
                tenantStatus.LastAppliedMigration = appliedMigrations.LastOrDefault();
                tenantStatus.PendingMigrations = pendingMigrations.ToArray();
                tenantStatus.HasPendingMigrations = tenantStatus.PendingMigrations.Count > 0;
            }
            // Per-tenant failure must not stop reporting on other tenants
            catch (Exception ex)
            {
                tenantStatus.Error = ex.Message;
            }

            tenantMigrationStatuses.Add(tenantStatus);
        }

        return tenantMigrationStatuses;
    }
}