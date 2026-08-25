using DreamTeam.Modules.Identity.Contracts.Authorization;
using DreamTeam.Modules.Identity.Domain;
using DreamTeam.Modules.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Identity.Tests.Services;

/// <summary>
/// Unit tests for <see cref="FdsRoleSeedService"/>. Verifies the
/// idempotency contract (existing roles are skipped) and the
/// create-on-missing path (new roles are created with the FDS
/// description).
/// </summary>
public sealed class FdsRoleSeedServiceTests
{
    /// <summary>
    /// Build a fresh test fixture: a scoped <see cref="RoleManager{T}"/>
    /// mock and a root <see cref="IServiceProvider"/> mock that returns it.
    /// </summary>
    private static (FdsRoleSeedService sut, RoleManager<DreamTeamRole> roleManager, IServiceScope scope)
        BuildSut(Func<string, bool> roleExistsPredicate)
    {
        var roleManager = Substitute.For<RoleManager<DreamTeamRole>>(
            Substitute.For<IRoleStore<DreamTeamRole>>(),
            null, null, null, null);

        roleManager.RoleExistsAsync(Arg.Any<string>())
            .Returns(ci => roleExistsPredicate(ci.ArgAt<string>(0) ?? string.Empty));

        roleManager.CreateAsync(Arg.Any<DreamTeamRole>())
            .Returns(IdentityResult.Success);

        // Build a real ServiceProvider so the scope-returning contract
        // works. We register a single descriptor for RoleManager<>.
        var services = new ServiceCollection();
        services.AddSingleton(roleManager);
        var sp = services.BuildServiceProvider();

        var scope = sp.CreateScope();
        var logger = Substitute.For<ILogger<FdsRoleSeedService>>();
        var sut = new FdsRoleSeedService(sp, logger);
        return (sut, roleManager, scope);
    }

    [Fact]
    public async Task StartAsync_Creates_AllFourFdsRoles_WhenNoneExist()
    {
        // Arrange — no role exists
        var (sut, roleManager, scope) = BuildSut(_ => false);
        using (scope)
        {
            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            await roleManager.Received(FdsRoles.All.Count).CreateAsync(Arg.Any<DreamTeamRole>());

            // Each created role must have the canonical FDS name + description.
            foreach (var roleName in FdsRoles.All)
            {
                await roleManager.Received(1).CreateAsync(
                    Arg.Is<DreamTeamRole>(r =>
                        r.Name == roleName &&
                        r.Description == FdsRoles.Description(roleName)));
            }
        }
    }

    [Fact]
    public async Task StartAsync_IsIdempotent_SkipsExistingRoles()
    {
        // Arrange — every role already exists
        var (sut, roleManager, scope) = BuildSut(_ => true);
        using (scope)
        {
            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            await roleManager.DidNotReceiveWithAnyArgs().CreateAsync(default(DreamTeamRole)!);
            foreach (var roleName in FdsRoles.All)
            {
                await roleManager.Received(1).RoleExistsAsync(roleName);
            }
        }
    }

    [Fact]
    public async Task StartAsync_CreatesMissingRoles_AndSkipsExisting_OnMixedState()
    {
        // Arrange — only TeamLead exists; the rest are missing.
        var (sut, roleManager, scope) = BuildSut(name =>
            string.Equals(name, FdsRoles.TeamLead, StringComparison.Ordinal));
        using (scope)
        {
            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert — TeamLead was skipped, the other 3 were created.
            await roleManager.DidNotReceive().CreateAsync(
                Arg.Is<DreamTeamRole>(r => r.Name == FdsRoles.TeamLead));
            await roleManager.Received(1).CreateAsync(
                Arg.Is<DreamTeamRole>(r => r.Name == FdsRoles.PM));
            await roleManager.Received(1).CreateAsync(
                Arg.Is<DreamTeamRole>(r => r.Name == FdsRoles.DeliveryManager));
            await roleManager.Received(1).CreateAsync(
                Arg.Is<DreamTeamRole>(r => r.Name == FdsRoles.Member));
        }
    }

    [Fact]
    public async Task StartAsync_LogsAndContinues_WhenCreateFails()
    {
        // Arrange — CreateAsync always returns Failed.
        var roleManager = Substitute.For<RoleManager<DreamTeamRole>>(
            Substitute.For<IRoleStore<DreamTeamRole>>(),
            null, null, null, null);
        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(false);
        roleManager.CreateAsync(Arg.Any<DreamTeamRole>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "simulated failure" }));

        var services = new ServiceCollection();
        services.AddSingleton(roleManager);
        var sp = services.BuildServiceProvider();
        var scope = sp.CreateScope();
        var logger = Substitute.For<ILogger<FdsRoleSeedService>>();
        var sut = new FdsRoleSeedService(sp, logger);

        using (scope)
        {
            // Act — must NOT throw despite all 4 CreateAsync failing
            await sut.StartAsync(CancellationToken.None);

            // Assert — the loop continued past every failure (4 attempts).
            await roleManager.Received(FdsRoles.All.Count).CreateAsync(Arg.Any<DreamTeamRole>());
        }
    }

    [Fact]
    public async Task StartAsync_LogsAndContinues_WhenRoleManagerThrows()
    {
        // Arrange — RoleExistsAsync throws on the first call, returns
        // false on subsequent calls. The service must catch + log + keep
        // going for the remaining roles.
        var roleManager = Substitute.For<RoleManager<DreamTeamRole>>(
            Substitute.For<IRoleStore<DreamTeamRole>>(),
            null, null, null, null);

        var callCount = 0;
        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(_ =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new InvalidOperationException("simulated DB outage");
            }
            return false;
        });
        roleManager.CreateAsync(Arg.Any<DreamTeamRole>()).Returns(IdentityResult.Success);

        var services = new ServiceCollection();
        services.AddSingleton(roleManager);
        var sp = services.BuildServiceProvider();
        var scope = sp.CreateScope();
        var logger = Substitute.For<ILogger<FdsRoleSeedService>>();
        var sut = new FdsRoleSeedService(sp, logger);

        using (scope)
        {
            // Act — must not throw
            await sut.StartAsync(CancellationToken.None);

            // Assert — the loop continued past the first failure (3 more
            // RoleExistsAsync calls happened for the remaining FDS roles,
            // plus 3 successful CreateAsync calls).
            callCount.ShouldBe(FdsRoles.All.Count);
            await roleManager.Received(3).CreateAsync(Arg.Any<DreamTeamRole>());
        }
    }

    [Fact]
    public async Task StartAsync_RethrowsOperationCanceledException()
    {
        // Arrange — OperationCanceledException is the one exception we
        // DON'T swallow (host shutdown should propagate).
        var roleManager = Substitute.For<RoleManager<DreamTeamRole>>(
            Substitute.For<IRoleStore<DreamTeamRole>>(),
            null, null, null, null);
        roleManager.RoleExistsAsync(Arg.Any<string>())
            .Returns<bool>(_ => throw new OperationCanceledException());

        var services = new ServiceCollection();
        services.AddSingleton(roleManager);
        var sp = services.BuildServiceProvider();
        var scope = sp.CreateScope();
        var logger = Substitute.For<ILogger<FdsRoleSeedService>>();
        var sut = new FdsRoleSeedService(sp, logger);

        using (scope)
        {
            // Act + Assert
            await Should.ThrowAsync<OperationCanceledException>(
                async () => await sut.StartAsync(CancellationToken.None));
        }
    }
}
