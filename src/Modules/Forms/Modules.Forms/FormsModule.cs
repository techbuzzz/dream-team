using DreamTeam.Framework.Persistence;
using DreamTeam.Framework.Shared.Constants;
using DreamTeam.Framework.Web.Modules;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace DreamTeam.Modules.Forms;

/// <summary>
/// Forms module — the form engine per docs/architecture-v1.md §1-4. Owns
/// ProcessTemplate, FormVersion, ProcessInstance, Submission.
///
/// MVP-1 scaffolding only. The four entities are wired into the DbContext
/// and migration system, but the read/write endpoints (E1.4-E1.6 in the
/// roadmap — Builder, Renderer, 1-1 preset seed) land in follow-up
/// workstreams against this module. The Mediator marker is included so
/// handler discovery works once those endpoints are added.
/// </summary>
public sealed class FormsModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(FormsPermissions.All);

        builder.Services.AddHeroDbContext<FormsDbContext>();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<FormsDbContext>(
                name: "db:forms",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No middleware. The Forms module has no request-scoped middleware in MVP-1.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        // Endpoints land with the Builder (E1.5) and Renderer (E1.4) features.
        // Kept empty in the MVP-1 scaffold so the module loads cleanly.
    }
}
