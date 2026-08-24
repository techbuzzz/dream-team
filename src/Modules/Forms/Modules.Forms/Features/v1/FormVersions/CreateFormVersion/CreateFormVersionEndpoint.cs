using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.CreateFormVersion;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.CreateFormVersion;

public static class CreateFormVersionEndpoint
{
    public static RouteHandlerBuilder MapCreateFormVersionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/templates/{templateId:guid}/versions", async (
                IMediator mediator,
                [FromRoute] Guid templateId,
                [FromBody] CreateFormVersionRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateFormVersionCommand(
                    ProcessTemplateId: templateId,
                    Schema: request.Schema,
                    Description: request.Description);

                var result = await mediator.Send(command, cancellationToken);
                return TypedResults.Created(
                    $"/api/v1/forms/templates/{templateId}/versions/{result.VersionNumber}",
                    result);
            })
            .WithName("CreateFormVersion")
            .WithSummary("Publish a new form version against a process template")
            .RequirePermission(FormsPermissions.FormVersions.Publish)
            .WithDescription("Snapshot-on-publish: the JSON schema is frozen at this moment and becomes the live version for any future process instance. Returns 404 if the template does not exist, 409 if it is archived.")
            .Produces<FormVersionDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}

/// <summary>
/// Wire DTO for the endpoint. We accept the template id from the route and
/// the rest from the body, so a separate request record keeps the command
/// record's shape predictable (id from route, no envelope).
/// </summary>
public sealed record CreateFormVersionRequest(string Schema, string? Description);
