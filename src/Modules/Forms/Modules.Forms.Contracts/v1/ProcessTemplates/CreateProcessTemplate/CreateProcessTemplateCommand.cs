using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.CreateProcessTemplate;

/// <summary>
/// MVP-1 E1.1 — Scaffold: create a ProcessTemplate (the semantic wrapper
/// for a recurring ritual). The template starts with NO FormVersion; a
/// later command (PublishFormVersion) adds the first version. Slug is
/// required and unique per tenant.
///
/// Per docs/architecture-v1.md §1 + docs/processes.md (preset catalog:
/// "1-1", "Daily", "Retro", "Skill Wheel", "OKR Check-in").
/// </summary>
public sealed record CreateProcessTemplateCommand(
    string Name,
    string Slug,
    string? Description,
    string? Category)
    : ICommand<ProcessTemplateDto>;
