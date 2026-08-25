using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.UpdateProcessInstance;

/// <summary>
/// MVP-1 E1.1 (last missed slice) — PATCH-style content update for a
/// ProcessInstance. Both fields are optional; null means "leave
/// unchanged".
///
/// Mutability rules:
/// <list type="bullet">
///   <item><c>ScheduledAt</c> — when supplied, must be in the future
///         (validator enforces).</item>
///   <item><c>PairUserId</c> — when supplied, must be non-empty.</item>
///   <item><c>FormVersionId</c> — NOT updatable. Changing the snapshot
///         would invalidate any Submissions already on file; instead,
///         schedule a new instance against the newer version.</item>
/// </list>
///
/// State-machine guard: 409 if the instance is in a terminal state
/// (<c>Completed</c> or <c>Skipped</c>). The handler checks this before
/// calling the domain method.
/// </summary>
public sealed record UpdateProcessInstanceCommand(
    Guid Id,
    DateTime? ScheduledAt,
    string? PairUserId)
    : ICommand<ProcessInstanceDto>;
