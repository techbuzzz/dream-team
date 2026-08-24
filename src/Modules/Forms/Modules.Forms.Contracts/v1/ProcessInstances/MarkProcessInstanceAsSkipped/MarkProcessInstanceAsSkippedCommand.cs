using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.MarkProcessInstanceAsSkipped;

/// <summary>
/// MVP-1 E1.1 — Mark a ProcessInstance as Skipped. The "ritual is being
/// cancelled for this occurrence" action: a 1-1 for a teammate on vacation,
/// a Daily during a company off-site, a Retro that gets bumped by an incident.
///
/// Use cases:
/// <list type="bullet">
///   <item>Lead manually skips from the dashboard.</item>
///   <item>ScheduleException in MVP-2 (E2.5) — a holiday hits an instance
///         and the system auto-skips it (that flow lands in a later workstream).</item>
/// </list>
///
/// State machine: allowed from <c>Planned</c> or <c>Running</c>. Transitioning
/// from <c>Completed</c> or <c>Skipped</c> is rejected with 409 (idempotent
/// rejection: a second call doesn't double-skip, it just surfaces the
/// terminal state).
///
/// Named "Mark" rather than "Skip" to satisfy
/// <c>EndpointConventionTests.Endpoint_Names_Should_Follow_Convention</c> —
/// the verb allowlist does include "Skip" (used in FormsPermissions), but
/// the test for the endpoint class name only checks the class name pattern,
/// and "Mark" is the consistent state-transition verb we use for both
/// Completed and Skipped.
/// </summary>
public sealed record MarkProcessInstanceAsSkippedCommand(Guid InstanceId) : ICommand<ProcessInstanceDto>;
