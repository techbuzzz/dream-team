using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Users.GetUserGroups;

public sealed record GetUserGroupsQuery(string UserId) : IQuery<IEnumerable<GroupDto>>;