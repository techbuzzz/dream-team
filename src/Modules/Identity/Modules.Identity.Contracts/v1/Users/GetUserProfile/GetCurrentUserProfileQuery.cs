using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Users.GetUserProfile;

public sealed record GetCurrentUserProfileQuery(string UserId) : IQuery<UserDto>;