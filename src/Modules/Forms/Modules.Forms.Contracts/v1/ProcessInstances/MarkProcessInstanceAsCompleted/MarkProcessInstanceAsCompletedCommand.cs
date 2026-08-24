using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.MarkProcessInstanceAsCompleted;

/// <summary>
/// MVP-1 E1.1 — Mark a ProcessInstance as Completed. This is the
/// "ritual finished" action: the renderer / form has been processed and
/// the instance closes out.
///
/// Two valid sources of truth for completion:
/// <list type="number">
///   <item>Submitting a Submission (Submission.Submit) auto-transitions
///         the instance to Completed.</item>
///   <item>This explicit command — for rituals without a Submission
///         (e.g. async 1-1 where the lead just marks done in the dashboard).</item>
/// </list>
///
/// Allowed source states: <c>Planned</c> or <c>Running</c>. Transitioning
/// from <c>Completed</c> or <c>Skipped</c> is rejected with 409 (a closed
/// instance stays closed; to "reopen" we'd need a separate flow that
/// doesn't exist in MVP-1).
///
/// Named "Mark" rather than "Complete" to satisfy
/// <c>EndpointConventionTests.Endpoint_Names_Should_Follow_Convention</c>
/// — the verb allowlist (Get/Create/Update/Delete/Mark/Set/...) does not
/// include "Complete".
/// </summary>
public sealed record MarkProcessInstanceAsCompletedCommand(Guid InstanceId) : ICommand<ProcessInstanceDto>;
