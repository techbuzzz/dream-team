using DreamTeam.Modules.Forms.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamTeam.Modules.Forms.Data.Configurations;

public sealed class ProcessInstanceConfiguration : IEntityTypeConfiguration<ProcessInstance>
{
    public void Configure(EntityTypeBuilder<ProcessInstance> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("ProcessInstances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.PairUserId).HasMaxLength(64);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => new { x.TenantId, x.ScheduledAt });
        builder.HasIndex(x => new { x.TenantId, x.PairUserId, x.ScheduledAt });
        builder.HasIndex(x => new { x.TenantId, x.Status });

        builder.HasMany(x => x.Submissions)
            .WithOne(x => x.ProcessInstance!)
            .HasForeignKey(x => x.ProcessInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
