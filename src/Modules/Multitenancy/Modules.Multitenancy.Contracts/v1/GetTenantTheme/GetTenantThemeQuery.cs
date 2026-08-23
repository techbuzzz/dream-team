using DreamTeam.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Contracts.v1.GetTenantTheme;

public sealed record GetTenantThemeQuery : IQuery<TenantThemeDto>;