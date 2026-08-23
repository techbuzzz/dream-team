using DreamTeam.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Contracts.v1.TenantProvisioning;

public sealed record GetTenantProvisioningStatusQuery(string TenantId) : IQuery<TenantProvisioningStatusDto>;