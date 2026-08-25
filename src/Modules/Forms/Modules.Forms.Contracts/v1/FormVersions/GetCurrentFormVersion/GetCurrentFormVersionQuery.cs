using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetCurrentFormVersion;

/// <summary>
/// MVP-1 E1.1 (missed slice) — Fetch the currently-live FormVersion for a
/// template. "Current" = the row with <c>IsCurrent = true</c>; the
/// snapshot-on-publish invariant ensures exactly one such row per template
/// (enforced by a partial unique index in <c>FormVersionConfiguration</c>).
///
/// Returns 404 if either:
/// <list type="bullet">
///   <item>the template does not exist in the caller's tenant, or</item>
///   <item>the template exists but no FormVersion has been published yet.</item>
/// </list>
/// The renderer treats both as "no form to display" — the dashboard
/// surfaces "no published form" UX and prompts the lead to publish.
///
/// This is a read-through convenience over
/// <c>GetFormVersionsByTemplateId(templateId).IsCurrent=true.FirstOrDefault()</c>
/// — saves a round-trip and avoids the renderer re-implementing the
/// partial-unique-index contract.
/// </summary>
public sealed record GetCurrentFormVersionQuery(Guid TemplateId) : IQuery<FormVersionDto>;
