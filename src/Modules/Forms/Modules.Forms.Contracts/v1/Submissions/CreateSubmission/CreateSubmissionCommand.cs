using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.Submissions.CreateSubmission;

/// <summary>
/// MVP-1 E1.1 — Submit a response to a ProcessInstance. This is the
/// "fill the form and save" primitive that closes the loop on the form
/// engine.
///
/// Atomic side effect: successfully submitting a Submission auto-transitions
/// the target ProcessInstance to <c>Completed</c> (in the same SaveChanges
/// transaction). A second submission attempt to the same instance returns
/// 409 — the instance is now terminal.
///
/// Append-only correction flow: to amend a prior submission, the caller
/// sends a new <c>CreateSubmissionCommand</c> with
/// <c>CompensatesSubmissionId</c> set to the prior Submission.Id. The
/// original row is never mutated; the new row's <c>IsCompensating</c> is
/// derived from <c>CompensatesSubmissionId != null</c>.
///
/// The FormVersionId on the command must equal the ProcessInstance's
/// snapshot FormVersionId (snapshot-on-publish invariant). A mismatch is
/// a 409 — it means the caller filled the form against a newer version
/// than the instance is bound to.
/// </summary>
public sealed record CreateSubmissionCommand(
    Guid ProcessInstanceId,
    Guid FormVersionId,
    string Data,
    Guid? CompensatesSubmissionId = null)
    : ICommand<SubmissionDto>;
