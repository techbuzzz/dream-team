using System.Collections.ObjectModel;
using DreamTeam.Framework.Shared.Constants;
using DreamTeam.Modules.Identity.Contracts.Authorization;

namespace DreamTeam.Modules.Identity.Authorization;

/// <summary>
/// E1.2 slice 3 — FDS role-to-permission policy map. Maps the 4 FDS roles
/// (TeamLead / PM / DeliveryManager / Member) to the permission strings
/// they should hold across the Forms module.
///
/// Permission strings use the canonical
/// <c>"Permissions.{Resource}.{Action}"</c> shape (mirrored from how
/// <c>FormsPermissions</c> and <c>IdentityPermissions</c> name their
/// constants) so the existing
/// <see cref="RolePermissionSyncHostedService"/> picks them up on next
/// run. The sync service only ADDS missing claims; this bootstrap
/// service defines the canonical set per role.
///
/// RBAC model (per docs/processes.md and the FDS):
/// <list type="bullet">
///   <item><b>TeamLead</b> — full Forms CRUD for their team. The primary
///         1-1 actor. Can publish form versions, schedule instances,
///         mark complete/skip, read submissions.</item>
///   <item><b>PM</b> — same shape as TeamLead, cross-team. MVP-1 RBAC
///         doesn't model team scope (single-tenant per the prep), so
///         PM and TeamLead currently hold the same Forms permission set;
///         team-scope filtering lands in MVP-2/v4 (multitenancy).</item>
///   <item><b>DeliveryManager</b> — read + intervene. Can view all
///         Forms data, archive templates (Delete), and skip/complete
///         instances. Does NOT create new templates (that stays with
///         the lead/PM who owns the ritual).</item>
///   <item><b>Member</b> — read-only on templates + versions; can fill
///         their own 1-1 submissions. Cannot publish, cannot create
///         templates, cannot delete or skip instances.</item>
/// </list>
///
/// Sits in Identity (not Identity.Contracts) because it cross-references
/// Framework.Identity constants (ActionConstants). The contract layer
/// stays implementation-agnostic.
/// </summary>
public static class FdsRolePolicies
{
    /// <summary>
    /// Permission string format — single source of truth (mirrors
    /// the format used by FormsPermissions / IdentityPermissions in the
    /// identity catalog). Cached as a <see cref="System.Text.CompositeFormat"/> to
    /// satisfy CA1863 (and to be allocation-free on the bootstrap path,
    /// which iterates over every role × every policy).
    /// </summary>
    private static readonly System.Text.CompositeFormat PermissionFormat =
        System.Text.CompositeFormat.Parse("Permissions.{0}.{1}");

    private static string Perm(string resource, string action) =>
        string.Format(System.Globalization.CultureInfo.InvariantCulture, PermissionFormat, resource, action);

    // Forms resources. Hard-coded strings to avoid a hard project
    // reference on the static-initialiser path; we cross-check at
    // runtime in FdsRolePolicyBootstrap that these match
    // FormsPermissions.*.Resource (logged warning on drift).
    private const string ProcessTemplates = "ProcessTemplates";
    private const string FormVersions     = "FormVersions";
    private const string ProcessInstances = "ProcessInstances";
    private const string Submissions       = "Submissions";

    /// <summary>
    /// All four role policies. Order doesn't matter (the bootstrap service
    /// applies them independently).
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> All { get; } =
        new ReadOnlyDictionary<string, IReadOnlyList<string>>(
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
            {
                [FdsRoles.TeamLead] = new[]
                {
                    // Full Forms CRUD on instances (including manual mark-done via Complete)
                    Perm(ProcessTemplates, ActionConstants.View),
                    Perm(ProcessTemplates, ActionConstants.Create),
                    Perm(ProcessTemplates, ActionConstants.Update),
                    Perm(ProcessTemplates, ActionConstants.Delete),
                    Perm(FormVersions, ActionConstants.View),
                    Perm(FormVersions, "Publish"),
                    Perm(ProcessInstances, ActionConstants.View),
                    Perm(ProcessInstances, ActionConstants.Create),
                    Perm(ProcessInstances, "Skip"),
                    Perm(ProcessInstances, "Complete"),
                    Perm(Submissions, ActionConstants.View),
                },
                [FdsRoles.PM] = new[]
                {
                    // PM == TeamLead in MVP-1 (no team-scope yet).
                    Perm(ProcessTemplates, ActionConstants.View),
                    Perm(ProcessTemplates, ActionConstants.Create),
                    Perm(ProcessTemplates, ActionConstants.Update),
                    Perm(ProcessTemplates, ActionConstants.Delete),
                    Perm(FormVersions, ActionConstants.View),
                    Perm(FormVersions, "Publish"),
                    Perm(ProcessInstances, ActionConstants.View),
                    Perm(ProcessInstances, ActionConstants.Create),
                    Perm(ProcessInstances, "Skip"),
                    Perm(ProcessInstances, "Complete"),
                    Perm(Submissions, ActionConstants.View),
                },
                [FdsRoles.DeliveryManager] = new[]
                {
                    // Read + intervene (no Create on templates OR instances — the lead owns the ritual).
                    Perm(ProcessTemplates, ActionConstants.View),
                    Perm(ProcessTemplates, ActionConstants.Update),
                    Perm(ProcessTemplates, ActionConstants.Delete),
                    Perm(FormVersions, ActionConstants.View),
                    Perm(ProcessInstances, ActionConstants.View),
                    Perm(ProcessInstances, "Skip"),
                    Perm(ProcessInstances, "Complete"),
                    Perm(Submissions, ActionConstants.View),
                },
                [FdsRoles.Member] = new[]
                {
                    // Read templates + fill + amend own submissions.
                    Perm(FormVersions, ActionConstants.View),
                    Perm(ProcessInstances, ActionConstants.View),
                    Perm(Submissions, ActionConstants.View),
                    Perm(Submissions, ActionConstants.Create),
                    Perm(Submissions, ActionConstants.Update),
                },
            });

    /// <summary>
    /// Per-role policy lookup; returns an empty list for unknown roles
    /// (rather than throwing) so the bootstrap service logs and continues.
    /// </summary>
    public static IReadOnlyList<string> PolicyFor(string roleName) =>
        All.TryGetValue(roleName, out var p) ? p : Array.Empty<string>();
}
