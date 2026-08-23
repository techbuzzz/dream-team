using DreamTeam.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Contracts.v1.TenantProvisioning;

public sealed record RetryTenantProvisioningCommand(string TenantId) : ICommand<TenantProvisioningStatusDto>;