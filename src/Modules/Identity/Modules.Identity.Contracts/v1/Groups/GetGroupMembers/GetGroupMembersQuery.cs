using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Groups.GetGroupMembers;

public sealed record GetGroupMembersQuery(Guid GroupId) : IQuery<IEnumerable<GroupMemberDto>>;