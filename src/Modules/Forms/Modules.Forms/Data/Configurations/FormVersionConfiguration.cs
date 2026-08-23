using DreamTeam.Modules.Forms.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamTeam.Modules.Forms.Data.Configurations;

public sealed class FormVersionConfiguration : IEntityTypeConfiguration<FormVersion>
{
    public void Configure(EntityTypeBuilder<FormVersion> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("FormVersions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.VersionNumber).IsRequired();
        builder.Property(x => x.Schema).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.PublishedById).IsRequired().HasMaxLength(64);

        // Per (template, version-number) unique; per (template, is-current) unique
        // (so only one version is "current" at a time).
        builder.HasIndex(x => new { x.TenantId, x.ProcessTemplateId, x.VersionNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.ProcessTemplateId, x.IsCurrent })
            .HasFilter("\"IsCurrent\" = true")
            .IsUnique();

        builder.HasMany(x => x.ProcessInstances)
            .WithOne(x => x.FormVersion!)
            .HasForeignKey(x => x.FormVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
