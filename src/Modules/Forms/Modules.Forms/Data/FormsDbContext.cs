using DreamTeam.Framework.Persistence.Context;
using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Domain;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DreamTeam.Modules.Forms.Data;

/// <summary>
/// Forms module DbContext. Per the FDS, the form engine has four entities:
///   - ProcessTemplate  (semantic wrapper for a recurring ritual)
///   - FormVersion      (immutable schema snapshot)
///   - ProcessInstance  (one occurrence of a template)
///   - Submission       (append-only response)
///
/// MVP-1 is single-tenant: every entity is tenant-scoped via the BaseDbContext's
/// default-on <c>ApplyTenantIsolationByDefault()</c> filter. FormVersion is
/// the only one that would be marked IGlobalEntity in a future v4 if we add
/// a marketplace of public forms.
/// </summary>
public class FormsDbContext : BaseDbContext
{
    public const string Schema = "forms";

    public FormsDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<FormsDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment)
        : base(multiTenantContextAccessor, options, settings, environment)
    {
    }

    public DbSet<ProcessTemplate> ProcessTemplates => Set<ProcessTemplate>();
    public DbSet<FormVersion> FormVersions => Set<FormVersion>();
    public DbSet<ProcessInstance> ProcessInstances => Set<ProcessInstance>();
    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FormsDbContext).Assembly);
        // MUST be last — applies tenant + soft-delete filters AFTER per-entity configs.
        base.OnModelCreating(modelBuilder);
    }
}
