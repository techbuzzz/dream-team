using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Roles.GetRole;

public sealed record GetRoleQuery(string Id) : IQuery<RoleDto?>;