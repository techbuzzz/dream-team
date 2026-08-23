using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Modules.Multitenancy.Provisioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamTeam.Modules.Multitenancy.Data.Configurations;

public class TenantProvisioningStepConfiguration : IEntityTypeConfiguration<TenantProvisioningStep>
{
    public void Configure(EntityTypeBuilder<TenantProvisioningStep> builder)
    {
        builder.ToTable("TenantProvisioningSteps", MultitenancyConstants.Schema);
    }
}