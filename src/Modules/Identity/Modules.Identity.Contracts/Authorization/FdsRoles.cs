using System.Collections.ObjectModel;

namespace DreamTeam.Modules.Identity.Contracts.Authorization;

/// <summary>
/// FDS (Functional Design Spec) role names. The product hierarchy per
/// docs/processes.md and the FDS roadmap:
/// <list type="bullet">
///   <item><b>TeamLead</b> — owns 1-1 rituals with their team members. Can
///         configure process templates, schedule instances, and read
///         their team's dashboard. Per the FDS, a TeamLead is the
///         primary actor in a 1-1.</item>
///   <item><b>PM</b> — Product Manager. Cross-team: configures the
///         full template library (Daily, Retro, Skill Wheel, OKR),
///         reads cross-team dashboards, and is the natural owner of
///         the Renderer + Builder UX. Manages 3-5 teams.</item>
///   <item><b>DeliveryManager</b> — owns the delivery process across
///         teams. Reads cross-team dashboards, can archive + intervene
///         on any team. Manages 10-15 teams (PM scale-out).</item>
///   <item><b>Member</b> — the regular team member. Reads + fills
///         forms scheduled for them, sees only their own dashboard
///         (their 1-1s with their lead).</item>
/// </list>
///
/// These names live as plain strings (mirroring the framework's
/// RoleConstants style) so the underlying DreamTeamRole entity can be
/// seeded with them in a follow-up migration. Role-to-permission
/// mapping is a separate concern (RBAC policies, future workstream) —
/// the 4 names here are just the contract.
///
/// Sits in Identity.Contracts (not Domain) so other modules (Forms,
/// future Rituals, etc.) can reference the role names without taking
/// a hard dependency on the FSH-sourced Identity internals.
/// </summary>
public static class FdsRoles
{
    public const string TeamLead = nameof(TeamLead);
    public const string PM = nameof(PM);
    public const string DeliveryManager = nameof(DeliveryManager);
    public const string Member = nameof(Member);

    /// <summary>
    /// Aggregate registry — every FDS role. Used by the seed migration
    /// (and by future RBAC-policy code that needs to enumerate roles
    /// without hard-coding the count).
    /// </summary>
    public static IReadOnlyList<string> All { get; } = new ReadOnlyCollection<string>(new[]
    {
        TeamLead,
        PM,
        DeliveryManager,
        Member,
    });

    /// <summary>
    /// Returns true if <paramref name="roleName"/> is one of the FDS-defined
    /// roles. Useful for input validation and for "is this user a FDS
    /// product role (vs. an internal Admin/Basic)" checks.
    /// </summary>
    public static bool IsFdsRole(string roleName) => All.Contains(roleName);

    /// <summary>
    /// Per-role description — surfaces in the admin UI's role catalog
    /// ("TeamLead — Owns 1-1 rituals with their team members").
    /// </summary>
    public static string Description(string roleName) => roleName switch
    {
        TeamLead        => "Owns 1-1 rituals with their team members. Can configure process templates and schedule instances for their team.",
        PM              => "Product Manager. Cross-team: configures the full template library and reads cross-team dashboards.",
        DeliveryManager => "Owns the delivery process across teams. Reads cross-team dashboards and can archive or intervene on any team.",
        Member          => "Regular team member. Reads and fills forms scheduled for them; sees only their own dashboard.",
        _               => "Unknown FDS role.",
    };
}
