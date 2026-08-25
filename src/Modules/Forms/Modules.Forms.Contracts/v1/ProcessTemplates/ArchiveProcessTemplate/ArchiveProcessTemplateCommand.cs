using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.ArchiveProcessTemplate;

/// <summary>
/// MVP-1 E1.1 (missed slice) — Soft-archive a ProcessTemplate. Flips
/// <c>IsArchived = true</c>; the row stays in the database and the
/// FormVersion chain stays intact (existing ProcessInstances and
/// Submissions keep working). The template is hidden from the default
/// list and rejected by mutation endpoints with 409.
///
/// Returns 404 if missing, 409 if already archived (idempotency is
/// surfaced as an error to match the Mark* state-transition commands
/// — a future Unarchive flow reverses this).
///
/// Permission: reuses <c>FormsPermissions.ProcessTemplates.Delete</c>
/// — the semantic intent ("put this template away") aligns with delete;
/// introducing a dedicated Archive permission is out of scope for the
/// MVP-1 sub-slice backlog.
/// </summary>
public sealed record ArchiveProcessTemplateCommand(Guid Id) : ICommand<ProcessTemplateDto>;
