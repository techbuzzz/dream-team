using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Users.GetUserPermissions;

public sealed record GetCurrentUserPermissionsQuery(string UserId) : IQuery<List<string>?>;