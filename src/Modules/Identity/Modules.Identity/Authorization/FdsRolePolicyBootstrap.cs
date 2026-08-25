using DreamTeam.Modules.Identity.Contracts.Authorization;
using DreamTeam.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DreamTeam.Modules.Identity.Authorization;

/// <summary>
/// E1.2 slice 3 — Applies the <see cref="FdsRolePolicies"/> map to the
/// 4 FDS roles in the database. Runs as a hosted service AFTER
/// <see cref="DreamTeam.Modules.Identity.Services.FdsRoleSeedService"/>
/// (the seeder is registered first in the DI order so the policy
/// bootstrap always finds the roles to attach claims to).
///
/// Idempotent: every claim already attached to a role is skipped.
/// The intent is to add NEW claims (e.g. a future slice adds a
/// "Permissions.ProcessInstances.Complete" permission to TeamLead and
/// the bootstrap picks it up on next boot). Removing a claim from
/// <see cref="FdsRolePolicies"/> does NOT remove it from the database
/// here — that's an explicit admin action via
/// <c>POST /api/v1/identity/roles/{id}/permissions</c> (Delete action).
///
/// Tenant scope: FDS roles are global (not per-tenant), so this service
/// reads/writes against the catalog IdentityDbContext once, not per
/// tenant. The framework's <see cref="RolePermissionSyncHostedService"/>
/// already handles the per-tenant Admin/Basic syncing.
/// </summary>
public sealed class FdsRolePolicyBootstrap : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FdsRolePolicyBootstrap> _logger;

    public FdsRolePolicyBootstrap(IServiceProvider serviceProvider, ILogger<FdsRolePolicyBootstrap> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var roleManager = sp.GetRequiredService<RoleManager<DreamTeamRole>>();
        var dbContext = sp.GetRequiredService<DreamTeam.Modules.Identity.Data.IdentityDbContext>();

        foreach (var roleName in FdsRoles.All)
        {
            try
            {
                var role = await roleManager.Roles
                    .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
                    .ConfigureAwait(false);
                if (role is null)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning(
                            "FDS role {Role} not found — skipping policy bootstrap. " +
                            "FdsRoleSeedService should have created it; check DI registration order.",
                            roleName);
                    }
                    continue;
                }

                var target = FdsRolePolicies.PolicyFor(roleName);
                if (target.Count == 0)
                {
                    continue;
                }

                // Compute the set of claim values already attached to the role.
                // ClaimValue is non-nullable in the schema (RoleClaim.ClaimType
                // and ClaimValue are required), but the EF query is projected
                // through a nullable-annotation gap; we filter nulls to satisfy
                // the nullable analyser.
                var existing = await dbContext.RoleClaims
                    .Where(rc => rc.RoleId == role.Id && rc.ClaimValue != null)
                    .Select(rc => rc.ClaimValue!)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                var existingSet = new HashSet<string>(existing, StringComparer.Ordinal);

                var added = 0;
                foreach (var perm in target)
                {
                    if (existingSet.Contains(perm))
                    {
                        continue;
                    }

                    var result = await roleManager.AddClaimAsync(
                        role,
                        new System.Security.Claims.Claim("permission", perm))
                        .ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        added++;
                    }
                    else if (_logger.IsEnabled(LogLevel.Error))
                    {
                        var errs = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError(
                            "Failed to add claim {Perm} to role {Role}: {Errors}",
                            perm, roleName, errs);
                    }
                }

                if (added > 0 && _logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Bootstrap added {Count} permission claim(s) to FDS role {Role}.",
                        added, roleName);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex, "Exception while bootstrapping FDS role policy for {Role}.", roleName);
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
