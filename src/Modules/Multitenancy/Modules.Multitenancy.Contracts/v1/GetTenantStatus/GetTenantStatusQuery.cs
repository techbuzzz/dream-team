using DreamTeam.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Contracts.v1.GetTenantStatus;

public sealed record GetTenantStatusQuery(string TenantId) : IQuery<TenantStatusDto>;