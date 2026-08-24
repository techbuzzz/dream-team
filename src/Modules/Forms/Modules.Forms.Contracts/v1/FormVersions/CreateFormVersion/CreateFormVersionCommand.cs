using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.FormVersions.CreateFormVersion;

/// <summary>
/// MVP-1 E1.1 — Publish a new FormVersion against an existing ProcessTemplate.
/// This is the <b>snapshot-on-publish</b> primitive (per docs/architecture-v1.md §1
/// and Golden Rule #2 in AGENTS.md): the JSON schema is fixed at this moment,
/// any future edits create a new version. Every ProcessInstance that points at
/// this version renders the same form forever.
///
/// Class + endpoint are named "CreateFormVersion" to satisfy the
/// <c>EndpointConventionTests.Endpoint_Names_Should_Follow_Convention</c>
/// guard (verb-noun). The "publish" semantics live in the handler: it flips
/// the previous current version to non-current and writes the new one with
/// <c>IsCurrent = true</c> in a single SaveChanges.
///
/// Behaviour:
/// <list type="bullet">
///   <item>Resolves the template by id; 404 if missing / soft-deleted.</item>
///   <item>Refuses to publish against an archived template (409).</item>
///   <item>Computes <c>versionNumber = max(existing) + 1</c> (or 1 if none).</item>
///   <item>Atomically flips the previous current version's <c>IsCurrent = false</c>.</item>
///   <item>Persists the new version with <c>IsCurrent = true</c>.</item>
/// </list>
///
/// Concurrency: a unique index on (TenantId, ProcessTemplateId, VersionNumber) is
/// the last line of defence — two concurrent publishes on the same template may
/// race on <c>versionNumber</c>; the loser gets a 500 (DbUpdateException on
/// unique_violation). For MVP-1 single-editor flow this is acceptable. A future
/// tick may wrap it in a serializable transaction or add a "currentVersionId"
/// pointer on ProcessTemplate for optimistic-concurrency control.
/// </summary>
public sealed record CreateFormVersionCommand(
    Guid ProcessTemplateId,
    string Schema,
    string? Description)
    : ICommand<FormVersionDto>;
