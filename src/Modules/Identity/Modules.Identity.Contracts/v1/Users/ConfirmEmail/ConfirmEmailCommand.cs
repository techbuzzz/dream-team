using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Users.ConfirmEmail;

public sealed record ConfirmEmailCommand(string UserId, string Code, string Tenant) : ICommand<string>;