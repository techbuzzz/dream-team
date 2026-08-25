using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.ArchiveProcessTemplate;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.ArchiveProcessTemplate;

public static class ArchiveProcessTemplateEndpoint
{
    public static RouteHandlerBuilder MapArchiveProcessTemplateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        // Action endpoint: POST is used (not DELETE) because the operation
        // is a soft archive — the row stays in the DB, the FormVersion chain
        // stays intact, and the action is reversible via a future Unarchive
        // endpoint. POST /archive maps cleanly to "perform this action".
        return endpoints.MapPost("/templates/{templateId:guid}/archive", async (
                IMediator mediator,
                [FromRoute] Guid templateId,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new ArchiveProcessTemplateCommand(templateId), cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("ArchiveProcessTemplate")
            .WithSummary("Soft-archive a process template (reversible)")
            .RequirePermission(FormsPermissions.ProcessTemplates.Delete)
            .WithDescription("Sets IsArchived = true. The template is hidden from the default list and rejected by mutation endpoints. Existing ProcessInstances and Submissions continue to work — the FormVersion chain is preserved. 404 if missing, 409 if already archived.")
            .Produces<ProcessTemplateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}
