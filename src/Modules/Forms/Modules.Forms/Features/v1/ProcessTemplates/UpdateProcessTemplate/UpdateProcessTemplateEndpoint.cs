using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.UpdateProcessTemplate;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.UpdateProcessTemplate;

public static class UpdateProcessTemplateEndpoint
{
    public static RouteHandlerBuilder MapUpdateProcessTemplateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        // PATCH for content updates (Name, Description, Category). PATCH
        // semantics: at least one field must be supplied; nulls = leave
        // unchanged. Slug and OwnerId are not mutable from this path.
        return endpoints.MapPatch("/templates/{templateId:guid}", async (
                IMediator mediator,
                [FromRoute] Guid templateId,
                [FromBody] UpdateProcessTemplateRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateProcessTemplateCommand(
                    Id: templateId,
                    Name: request.Name,
                    Description: request.Description,
                    Category: request.Category);

                var result = await mediator.Send(command, cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("UpdateProcessTemplate")
            .WithSummary("Update a process template (PATCH — Name, Description, Category)")
            .RequirePermission(FormsPermissions.ProcessTemplates.Update)
            .WithDescription("PATCH-style content update. All three fields are optional; null = leave unchanged. Returns 400 if no field is supplied, 404 if the template does not exist, 409 if it is archived. Slug and OwnerId are not mutable from this endpoint.")
            .Produces<ProcessTemplateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}

/// <summary>
/// Wire DTO. Kept separate from the command record so the command's
/// parameter list is the canonical request shape. All fields optional
/// for PATCH semantics.
/// </summary>
public sealed record UpdateProcessTemplateRequest(
    string? Name,
    string? Description,
    string? Category);
