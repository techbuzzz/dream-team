using DreamTeam.Modules.Forms.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamTeam.Modules.Forms.Data.Configurations;

public sealed class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("Submissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.AuthorId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Data).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.IsCompensating).IsRequired().HasDefaultValue(false);
        builder.HasIndex(x => new { x.TenantId, x.ProcessInstanceId, x.AuthorId });
        builder.HasIndex(x => new { x.TenantId, x.CompensatesSubmissionId });
    }
}
