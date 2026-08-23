using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Sessions.GetMySessions;

public sealed record GetMySessionsQuery : IQuery<List<UserSessionDto>>;