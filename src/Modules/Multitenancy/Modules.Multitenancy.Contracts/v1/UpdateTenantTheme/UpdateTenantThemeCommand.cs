using DreamTeam.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Contracts.v1.UpdateTenantTheme;

public sealed record UpdateTenantThemeCommand(TenantThemeDto Theme) : ICommand;