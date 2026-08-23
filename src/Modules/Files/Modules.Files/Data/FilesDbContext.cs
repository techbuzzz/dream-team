using Finbuckle.MultiTenant.Abstractions;
using DreamTeam.Framework.Persistence.Context;
using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Files.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DreamTeam.Modules.Files.Data;

public sealed class FilesDbContext : BaseDbContext
{
    public const string Schema = "files";

    public FilesDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<FilesDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment) : base(multiTenantContextAccessor, options, settings, environment) { }

    public DbSet<FileAsset> FileAssets => Set<FileAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FilesDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply sees
        // fully-configured entities (including HasMany child types).
        base.OnModelCreating(modelBuilder);
    }
}
