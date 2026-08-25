using DreamTeam.Framework.Shared.Constants;

namespace DreamTeam.Modules.Forms.Contracts.Authorization;

/// <summary>
/// Forms module permissions. Single source of truth — the <see cref="All"/>
/// registry below is the only list registered with
/// <see cref="PermissionConstants.Register"/>, so the host's permission
/// catalog (and any role-assignment UI) cannot drift from the strings
/// used by <c>.RequirePermission(...)</c>.
///
/// MVP-1 (E1.x): ProcessTemplates, FormVersions, ProcessInstances, Submissions.
/// Action set is intentionally minimal — features (CRUD endpoints) that
/// exercise them land in the Forms-module workstream.
/// </summary>
public static class FormsPermissions
{
    public static class ProcessTemplates
    {
        public const string Resource = nameof(ProcessTemplates);
        public const string View   = "View";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
    }

    public static class FormVersions
    {
        public const string Resource = nameof(FormVersions);
        public const string View    = "View";
        public const string Publish = "Publish";
    }

    public static class ProcessInstances
    {
        public const string Resource = nameof(ProcessInstances);
        public const string View     = "View";
        public const string Create   = "Create";
        public const string Skip     = "Skip";
        public const string Complete = "Complete";
    }

    public static class Submissions
    {
        public const string Resource = nameof(Submissions);
        public const string View   = "View";
        public const string Create = "Create";
        public const string Update = "Update";
    }

    /// <summary>
    /// Aggregate registry — registered with <see cref="PermissionConstants"/> at
    /// module startup so the host's permission catalog includes every Forms
    /// permission for role assignment. The Description column surfaces in the
    /// admin UI ("View Templates", "Publish Form Version", etc.).
    /// </summary>
    public static IReadOnlyList<DreamTeamPermission> All { get; } =
    [
        new("View Process Templates",   ProcessTemplates.View,   ProcessTemplates.Resource, IsBasic: true),
        new("Create Process Template",  ProcessTemplates.Create, ProcessTemplates.Resource),
        new("Update Process Template",  ProcessTemplates.Update, ProcessTemplates.Resource),
        new("Delete Process Template",  ProcessTemplates.Delete, ProcessTemplates.Resource),

        new("View Form Versions",       FormVersions.View,      FormVersions.Resource,    IsBasic: true),
        new("Publish Form Version",     FormVersions.Publish,   FormVersions.Resource),

        new("View Process Instances",   ProcessInstances.View,     ProcessInstances.Resource, IsBasic: true),
        new("Create Process Instance",  ProcessInstances.Create,   ProcessInstances.Resource),
        new("Skip Process Instance",    ProcessInstances.Skip,     ProcessInstances.Resource),
        new("Complete Process Instance", ProcessInstances.Complete, ProcessInstances.Resource),

        new("View Submissions",         Submissions.View,       Submissions.Resource,     IsBasic: true),
        new("Create Submission",        Submissions.Create,     Submissions.Resource),
        new("Update Submission",        Submissions.Update,     Submissions.Resource),
    ];
}
