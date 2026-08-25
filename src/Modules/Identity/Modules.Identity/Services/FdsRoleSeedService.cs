using DreamTeam.Modules.Identity.Contracts.Authorization;
using DreamTeam.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DreamTeam.Modules.Identity.Services;

/// <summary>
/// E1.2 slice 2 — Idempotent startup seeder for the 4 FDS role names
/// (TeamLead, PM, DeliveryManager, Member). On every API / DbMigrator
/// startup, this service checks <c>RoleManager&lt;DreamTeamRole&gt;</c>
/// for each FDS role; missing roles are created with the description
/// from <see cref="FdsRoles.Description"/> as a human-readable hint
/// in the admin UI.
///
/// Why a hosted service (and not an EF migration with InsertData):
/// <list type="bullet">
///   <item>Idempotent — running it twice doesn't fail (InsertData with
///         a unique key would 23505 on a repeat run; this is a no-op
///         if roles already exist).</item>
///   <item>No schema migration needed — we're not changing <c>Roles</c>,
///         just seeding rows. Keeps the FSH migration count at 9
///         (we don't want a 10th migration just for 4 seed rows).</item>
///   <item>Runs on every host (API, DbMigrator, AppHost) so any process
///         that boots into the Identity schema gets the FDS roles
///         without an explicit "dotnet ef database update" step.</item>
///   <item>Roles are config-level data, not transactional state. A
///         hosted service is the right home.</item>
/// </list>
///
/// Caveats:
/// <list type="bullet">
///   <item>If <c>RoleExistsAsync</c> or <c>CreateAsync</c> fails (e.g. DB
///         down at boot), the service logs and continues — it does NOT
///         throw, because we don't want to crash the host over a
///         seed that can be retried on the next startup.</item>
///   <item>Tenant scope: <c>DreamTeamRole</c> lives in the tenant-catalog
///         database (Identity is multi-tenant but role definitions are
///         catalog-wide per FSH). Seed runs against the catalog DbContext
///         on startup. Per-tenant role assignment is a separate concern
///         (handled by <c>UserRoleService</c>).</item>
/// </list>
/// </summary>
public sealed class FdsRoleSeedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FdsRoleSeedService> _logger;

    public FdsRoleSeedService(IServiceProvider serviceProvider, ILogger<FdsRoleSeedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Resolve RoleManager<DreamTeamRole> in a fresh scope. RoleManager
        // is registered as scoped (AddIdentity<,> wires it as scoped by
        // default), so a hosted-service root scope is not appropriate.
        using var scope = _serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<DreamTeamRole>>();

        foreach (var roleName in FdsRoles.All)
        {
            try
            {
                if (await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("FDS role {Role} already exists — skipping seed.", roleName);
                    }
                    continue;
                }

                var role = new DreamTeamRole(roleName, FdsRoles.Description(roleName));
                var result = await roleManager.CreateAsync(role).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Seeded FDS role {Role}.", roleName);
                    }
                }
                else if (_logger.IsEnabled(LogLevel.Error))
                {
                    // Pre-format the error list — guards against CA1873
                    // ("expensive argument evaluation when logging disabled").
                    // The result.Errors list is small (typically 0-3 entries),
                    // so the cost is negligible, but we still gate the format
                    // on the log level to satisfy the analyzer.
                    var errorDetails = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description));
                    _logger.LogError(
                        "Failed to seed FDS role {Role}: {Errors}",
                        roleName,
                        errorDetails);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Log and continue — we don't want to crash the host over
                // a seed. The next startup will retry. OperationCanceledException
                // is rethrown so the host's graceful-shutdown path still works.
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex, "Exception while seeding FDS role {Role}.", roleName);
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
