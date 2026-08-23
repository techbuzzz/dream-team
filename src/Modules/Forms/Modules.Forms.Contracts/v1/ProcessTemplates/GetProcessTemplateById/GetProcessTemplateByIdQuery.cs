using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.GetProcessTemplateById;

/// <summary>
/// MVP-1 E1.4 (Renderer) prerequisite: fetch a single process template
/// by its Id. Returns <see cref="ProcessTemplateDto"/> on hit, 404
/// (NotFoundException) on miss. No validator — the Id is route-bound
/// and validated by ASP.NET Core route constraints (:guid).
/// </summary>
public sealed record GetProcessTemplateByIdQuery(Guid Id) : IQuery<ProcessTemplateDto>;
