using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Roles.DeleteRole;

public sealed record DeleteRoleCommand(string Id) : ICommand<Unit>;