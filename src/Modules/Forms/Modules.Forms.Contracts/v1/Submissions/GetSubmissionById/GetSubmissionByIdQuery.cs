using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionById;

/// <summary>
/// MVP-1 E1.1 (missed slice) — Fetch a single Submission by its Id. The
/// base DbContext's default-on tenant-isolation query filter narrows the
/// lookup to the caller's tenant; an Id that exists in another tenant
/// surfaces as 404 (no cross-tenant existence leaks).
///
/// Read-only (AsNoTracking). Submissions are append-only — there is no
/// mutation endpoint for them, by Golden Rule #3. To amend a row, send
/// a new CreateSubmissionCommand with <c>CompensatesSubmissionId</c>
/// set; the original row is never updated.
/// </summary>
public sealed record GetSubmissionByIdQuery(Guid Id) : IQuery<SubmissionDto>;
