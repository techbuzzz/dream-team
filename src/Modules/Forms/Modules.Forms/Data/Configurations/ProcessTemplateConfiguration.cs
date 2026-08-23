using DreamTeam.Modules.Forms.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamTeam.Modules.Forms.Data.Configurations;

public sealed class ProcessTemplateConfiguration : IEntityTypeConfiguration<ProcessTemplate>
{
    public void Configure(EntityTypeBuilder<ProcessTemplate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("ProcessTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.OwnerId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Category).HasMaxLength(64);
        builder.Property(x => x.DeletedBy).HasMaxLength(64);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.HasIndex(x => new { x.TenantId, x.IsDeleted });

        builder.HasMany(x => x.FormVersions)
            .WithOne(x => x.ProcessTemplate!)
            .HasForeignKey(x => x.ProcessTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
