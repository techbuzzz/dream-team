using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Groups.GetGroupById;

public sealed record GetGroupByIdQuery(Guid Id) : IQuery<GroupDto>;