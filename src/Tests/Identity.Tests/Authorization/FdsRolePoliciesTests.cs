using DreamTeam.Framework.Shared.Constants;
using DreamTeam.Modules.Identity.Authorization;
using DreamTeam.Modules.Identity.Contracts.Authorization;
using Shouldly;
using Xunit;

namespace Identity.Tests.Authorization;

/// <summary>
/// Unit tests for the FDS role-to-permission policy map. Pins the
/// policy contract so a permission-intent change in code (e.g. giving
/// TeamLead the Complete action) requires an explicit test update
/// — not a silent policy drift.
/// </summary>
public sealed class FdsRolePoliciesTests
{
    [Fact]
    public void All_ContainsAllFourFdsRoles()
    {
        // Assert
        FdsRolePolicies.All.Count.ShouldBe(FdsRoles.All.Count);
        foreach (var role in FdsRoles.All)
        {
            FdsRolePolicies.All.ShouldContainKey(role);
        }
    }

    [Fact]
    public void All_Permissions_StartWithPermissionsPrefix()
    {
        // Assert — every permission string in every role's policy must
        // match the canonical format. The framework's permission lookup
        // and the RolePermissionSyncHostedService both rely on this shape.
        foreach (var kvp in FdsRolePolicies.All)
        {
            foreach (var perm in kvp.Value)
            {
                perm.ShouldStartWith("Permissions.");
            }
        }
    }

    [Fact]
    public void PolicyFor_ReturnsEmpty_ForUnknownRole()
    {
        // Act + Assert
        FdsRolePolicies.PolicyFor("not-a-role").ShouldBeEmpty();
        FdsRolePolicies.PolicyFor("").ShouldBeEmpty();
    }

    [Fact]
    public void TeamLead_HasFullFormsCrud_ExceptComplete()
    {
        // Arrange
        var perms = FdsRolePolicies.PolicyFor(FdsRoles.TeamLead);

        // Assert — TeamLead = primary 1-1 actor with full CRUD except Complete
        // (Complete is the user's own action when they submit, not the lead's
        // explicit "mark done" — see FdsRolePolicies doc on DeliveryManager).
        perms.ShouldContain($"Permissions.ProcessTemplates.{ActionConstants.View}");
        perms.ShouldContain($"Permissions.ProcessTemplates.{ActionConstants.Create}");
        perms.ShouldContain($"Permissions.ProcessTemplates.{ActionConstants.Update}");
        perms.ShouldContain($"Permissions.ProcessTemplates.{ActionConstants.Delete}");
        perms.ShouldContain("Permissions.FormVersions.View");
        perms.ShouldContain("Permissions.FormVersions.Publish");
        perms.ShouldContain("Permissions.ProcessInstances.View");
        perms.ShouldContain("Permissions.ProcessInstances.Skip");
        perms.ShouldContain("Permissions.Submissions.View");
        perms.ShouldNotContain("Permissions.ProcessInstances.Complete");
    }

    [Fact]
    public void PM_HasSamePermissionsAsTeamLead_InMvp1()
    {
        // Assert — PM == TeamLead in MVP-1 (no team-scope yet; both have
        // the same Forms CRUD set). When team-scope lands in MVP-2 the
        // policies will diverge; this test is the explicit signal.
        var teamLead = FdsRolePolicies.PolicyFor(FdsRoles.TeamLead).OrderBy(x => x).ToArray();
        var pm = FdsRolePolicies.PolicyFor(FdsRoles.PM).OrderBy(x => x).ToArray();

        pm.ShouldBe(teamLead);
    }

    [Fact]
    public void DeliveryManager_HasReadPlusIntervent_ButNoCreate()
    {
        // Arrange
        var perms = FdsRolePolicies.PolicyFor(FdsRoles.DeliveryManager);

        // Assert
        perms.ShouldContain($"Permissions.ProcessTemplates.{ActionConstants.View}");
        perms.ShouldContain($"Permissions.ProcessTemplates.{ActionConstants.Update}");
        perms.ShouldContain($"Permissions.ProcessTemplates.{ActionConstants.Delete}");
        perms.ShouldNotContain($"Permissions.ProcessTemplates.{ActionConstants.Create}");
        perms.ShouldContain("Permissions.ProcessInstances.Skip");
        perms.ShouldContain("Permissions.ProcessInstances.Complete");
    }

    [Fact]
    public void Member_HasReadOnlyTemplates_AndCanCreateSubmissions()
    {
        // Arrange
        var perms = FdsRolePolicies.PolicyFor(FdsRoles.Member);

        // Assert — Member is read-only on templates + versions, can fill
        // and amend their own submissions.
        perms.ShouldContain("Permissions.FormVersions.View");
        perms.ShouldNotContain("Permissions.FormVersions.Publish");
        perms.ShouldContain("Permissions.ProcessInstances.View");
        perms.ShouldNotContain("Permissions.ProcessInstances.Skip");
        perms.ShouldNotContain("Permissions.ProcessInstances.Complete");
        perms.ShouldContain("Permissions.Submissions.View");
        perms.ShouldContain($"Permissions.Submissions.{ActionConstants.Create}");
        perms.ShouldContain($"Permissions.Submissions.{ActionConstants.Update}");
    }

    [Fact]
    public void All_Permissions_AreUnique_WithinEachRole()
    {
        // Assert — no accidental duplicates in any role's policy.
        foreach (var kvp in FdsRolePolicies.All)
        {
            var distinct = kvp.Value.Distinct(StringComparer.Ordinal).Count();
            distinct.ShouldBe(kvp.Value.Count, $"role {kvp.Key} has duplicate permissions");
        }
    }
}
