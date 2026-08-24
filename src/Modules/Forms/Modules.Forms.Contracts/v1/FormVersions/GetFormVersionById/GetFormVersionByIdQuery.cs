using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionById;

/// <summary>
/// MVP-1 E1.1 — Fetch a single FormVersion by its Id. The base DbContext's
/// default-on tenant-isolation query filter narrows the lookup to the
/// caller's tenant; an Id that exists in another tenant surfaces as 404,
/// which is the right behavior (no cross-tenant existence leaks).
///
/// Per docs/architecture-v1.md §1: FormVersion is an immutable snapshot.
/// This query is read-only (AsNoTracking) — never use the result to drive
/// an update; create a new version instead.
/// </summary>
public sealed record GetFormVersionByIdQuery(Guid Id) : IQuery<FormVersionDto>;
