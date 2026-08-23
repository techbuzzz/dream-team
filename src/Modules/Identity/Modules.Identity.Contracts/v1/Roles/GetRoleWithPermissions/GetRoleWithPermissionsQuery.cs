using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Roles.GetRoleWithPermissions;

public sealed record GetRoleWithPermissionsQuery(string Id) : IQuery<RoleDto>;