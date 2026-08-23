using DreamTeam.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace DreamTeam.Modules.Identity.Contracts.v1.Impersonation.EndImpersonation;

public sealed record EndImpersonationCommand() : ICommand<TokenResponse>;
