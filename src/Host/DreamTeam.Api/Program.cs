using DreamTeam.Framework.Eventing;
using DreamTeam.Framework.Web;
using DreamTeam.Framework.Web.Modules;
using DreamTeam.Modules.Files;
using DreamTeam.Modules.Forms;
using DreamTeam.Modules.Identity;
using DreamTeam.Modules.Multitenancy;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Serialize enums as string names. Frontends mirror this as string unions.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

if (builder.Environment.IsProduction())
{
    static void Require(IConfiguration config, string key)
    {
        if (string.IsNullOrWhiteSpace(config[key]))
        {
            throw new InvalidOperationException($"Missing required configuration '{key}' in Production.");
        }
    }

    var config = builder.Configuration;
    Require(config, "DatabaseOptions:ConnectionString");
    Require(config, "CachingOptions:Redis");
    Require(config, "JwtOptions:SigningKey");
}

builder.Services.AddMediator(o =>
{
    o.ServiceLifetime = ServiceLifetime.Scoped;
    // List module types only — the Mediator source generator scans the
    // referenced assemblies for ICommand / IQuery / INotification and
    // their handlers. Listing the module type keeps Program.cs out of
    // the `DreamTeam.Modules.*.Features` namespace, which the
    // architecture tests forbid hosts from referencing.
    o.Assemblies = [
        typeof(IdentityModule),
        typeof(MultitenancyModule),
        typeof(DreamTeam.Modules.Files.FilesModule),
        typeof(DreamTeam.Modules.Forms.FormsModule)];
});

// MVP-1 modules: Identity, Multitenancy (dormant until v4), Files, Forms.
// Forms is the form engine (ProcessTemplate/FormVersion/ProcessInstance/Submission).
// Rituals/Notifications/Dashboard/Digest land in MVP-2.
var moduleAssemblies = new Assembly[]
{
    typeof(IdentityModule).Assembly,
    typeof(MultitenancyModule).Assembly,
    typeof(DreamTeam.Modules.Files.FilesModule).Assembly,
    typeof(DreamTeam.Modules.Forms.FormsModule).Assembly,
};

builder.AddHeroPlatform(o =>
{
    o.EnableCaching = true;
    o.EnableMailing = true;
    o.EnableJobs = true;
    o.EnableQuotas = true;
    o.EnableSse = true;
    o.EnableRealtime = true;
});

// The transactional outbox is framework infrastructure with exactly one owner
// (EventingDbContext), so the host registers it once for every module.
builder.Services.AddEventingCore(builder.Configuration);

// MVP-1: register the no-op audit contracts. The real implementations
// (event-sourcing-lite audit, per FDS docs/architecture-v1.md §Audit) land in
// MVP-2+ as a separate workstream and replace these singletons.
builder.Services.AddSingleton<DreamTeam.Framework.Shared.Identity.ISecurityAudit, DreamTeam.Framework.Shared.Identity.NoOpSecurityAudit>();
builder.Services.AddSingleton<DreamTeam.Framework.Shared.Identity.IAuditClient, DreamTeam.Framework.Shared.Identity.NoOpAuditClient>();

builder.AddModules(moduleAssemblies);

// Self-heal deployments carrying retired per-module `{module}-outbox-dispatcher` Hangfire recurring jobs
// (the outbox is now dispatched by OutboxDispatcherHostedService). No-op once the storage is clean.
builder.Services.AddHostedService<DreamTeam.Api.OrphanedOutboxRecurringJobCleanupService>();

// Demo data is provisioned by the DbMigrator's `seed` verb, not the API — the API never mutates data on startup.
// See src/Host/DreamTeam.DbMigrator/README.md.

var app = builder.Build();

// MVP-1: single-tenant — multitenancy middleware is OFF (v4 per FDS). Re-enable when multi-tenancy lands.
app.UseHeroPlatform(p =>
{
    p.MapModules = true;
    p.ServeStaticFiles = true;
    p.UseQuotas = true;
    p.MapSseEndpoints = true;
    p.MapRealtime = true;
});

app.MapGet("/", () => Results.Ok(new { message = "hello world!" }))
   .WithTags("PlayGround")
   .AllowAnonymous();
await app.RunAsync();
