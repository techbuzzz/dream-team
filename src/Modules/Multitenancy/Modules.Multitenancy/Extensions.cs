using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using DreamTeam.Modules.Multitenancy.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DreamTeam.Modules.Multitenancy;

public static class Extensions
{
    public static WebApplication UseHeroMultiTenantDatabases(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseMultiTenant();

        return app;
    }
}