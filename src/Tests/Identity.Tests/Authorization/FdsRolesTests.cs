using DreamTeam.Modules.Identity.Contracts.Authorization;
using Shouldly;
using Xunit;

namespace Identity.Tests.Authorization;

/// <summary>
/// Domain tests for the FDS role-name contract. The role names are the
/// anchor point for every FDS permission mapping and the seed migration —
/// these tests pin them so a typo in one place doesn't silently break
/// the cross-module role checks.
/// </summary>
public sealed class FdsRolesTests
{
    [Fact]
    public void All_FourFdsRoles_ArePresent()
    {
        // Assert
        FdsRoles.All.Count.ShouldBe(4);
        FdsRoles.All.ShouldContain(FdsRoles.TeamLead);
        FdsRoles.All.ShouldContain(FdsRoles.PM);
        FdsRoles.All.ShouldContain(FdsRoles.DeliveryManager);
        FdsRoles.All.ShouldContain(FdsRoles.Member);
    }

    [Fact]
    public void RoleNames_AreNotEmpty_AndMatchNameOfOperator()
    {
        // The string values must match nameof(...) exactly so that
        // "TeamLead" stays the canonical form. Drift here breaks the
        // permission catalog and the seed migration.
        FdsRoles.TeamLead.ShouldBe("TeamLead");
        FdsRoles.PM.ShouldBe("PM");
        FdsRoles.DeliveryManager.ShouldBe("DeliveryManager");
        FdsRoles.Member.ShouldBe("Member");
    }

    [Fact]
    public void IsFdsRole_True_ForEachRegisteredRole()
    {
        // Assert
        FdsRoles.IsFdsRole(FdsRoles.TeamLead).ShouldBeTrue();
        FdsRoles.IsFdsRole(FdsRoles.PM).ShouldBeTrue();
        FdsRoles.IsFdsRole(FdsRoles.DeliveryManager).ShouldBeTrue();
        FdsRoles.IsFdsRole(FdsRoles.Member).ShouldBeTrue();
    }

    [Fact]
    public void IsFdsRole_False_ForFrameworkRolesAndUnrelated()
    {
        // Assert
        FdsRoles.IsFdsRole("Admin").ShouldBeFalse();     // framework default
        FdsRoles.IsFdsRole("Basic").ShouldBeFalse();     // framework default
        FdsRoles.IsFdsRole("").ShouldBeFalse();
        FdsRoles.IsFdsRole("not-a-role").ShouldBeFalse();
    }

    [Fact]
    public void Description_ReturnsNonEmptyText_ForEachRegisteredRole()
    {
        // Assert
        FdsRoles.Description(FdsRoles.TeamLead).ShouldNotBeNullOrEmpty();
        FdsRoles.Description(FdsRoles.PM).ShouldNotBeNullOrEmpty();
        FdsRoles.Description(FdsRoles.DeliveryManager).ShouldNotBeNullOrEmpty();
        FdsRoles.Description(FdsRoles.Member).ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Description_ReturnsDefaultText_ForUnknownRole()
    {
        // Assert
        FdsRoles.Description("not-a-role").ShouldBe("Unknown FDS role.");
    }
}
