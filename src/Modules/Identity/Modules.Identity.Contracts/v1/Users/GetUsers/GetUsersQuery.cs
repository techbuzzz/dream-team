using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Users.GetUsers;

public sealed record GetUsersQuery : IQuery<List<UserDto>>;