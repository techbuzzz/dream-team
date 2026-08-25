using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.UpdateProcessTemplate;

/// <summary>
/// MVP-1 E1.1 (missed slice) — PATCH-style content update for a
/// ProcessTemplate. All three mutable fields are optional; null means
/// "leave unchanged". The handler rejects the request with 400 if
/// ALL three are null (a PATCH with no changes is a no-op; we surface
/// that explicitly rather than silently succeed).
///
/// Not mutable here: <c>Slug</c> (unique business key — change would
/// invalidate URLs and downstream ProcessInstances), <c>OwnerId</c>
/// (ownership transfer is a separate flow), <c>IsArchived</c> (covered
/// by a dedicated Archive endpoint).
///
/// Returns 404 if the template does not exist in the caller's tenant,
/// 409 if it is archived (archived templates are read-only — restore
/// first via a future Unarchive endpoint).
/// </summary>
public sealed record UpdateProcessTemplateCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? Category)
    : ICommand<ProcessTemplateDto>;
