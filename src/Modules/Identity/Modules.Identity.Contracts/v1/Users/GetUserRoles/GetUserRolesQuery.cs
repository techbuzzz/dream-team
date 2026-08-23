using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Users.GetUserRoles;

public sealed record GetUserRolesQuery(string UserId) : IQuery<List<UserRoleDto>>;