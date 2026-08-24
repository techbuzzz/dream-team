using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.CreateProcessInstance;

/// <summary>
/// MVP-1 E1.1 — Schedule a ProcessInstance against a published FormVersion.
/// This is the bridge primitive between the form engine and the future
/// Rituals module (E2.1 in MVP-2): a scheduled instance is what the
/// Renderer renders, and what the future Reminder Dispatcher (E2.4) fires
/// invitations for.
///
/// Class + endpoint are named "CreateProcessInstance" to satisfy the
/// <c>EndpointConventionTests.Endpoint_Names_Should_Follow_Convention</c>
/// guard (verb-noun). The "schedule" semantics live in the handler: it
/// creates a Planned instance with <c>ScheduledAt</c> from the command.
///
/// Constraints (enforced by the handler):
/// <list type="bullet">
///   <item>The FormVersion must exist in the caller's tenant (404 otherwise).</item>
///   <item>The FormVersion must be the live one (<c>IsCurrent = true</c>). MVP-1
///         refuses to schedule against historical versions — that's a
///         backfill / admin operation, not a normal schedule flow.</item>
/// </list>
///
/// <see cref="PairUserId"/> is optional: null for whole-team rituals
/// (Daily Sync, Sprint Retro); set for 1-1 between a lead and a member.
/// </summary>
public sealed record CreateProcessInstanceCommand(
    Guid FormVersionId,
    DateTime ScheduledAt,
    string? PairUserId = null)
    : ICommand<ProcessInstanceDto>;
