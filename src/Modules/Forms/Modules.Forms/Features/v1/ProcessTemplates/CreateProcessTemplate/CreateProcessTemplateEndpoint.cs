using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.CreateProcessTemplate;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.CreateProcessTemplate;

public static class CreateProcessTemplateEndpoint
{
    public static RouteHandlerBuilder MapCreateProcessTemplateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/templates", async (
            IMediator mediator,
            [FromBody] CreateProcessTemplateCommand request,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(request, cancellationToken);
            return TypedResults.Created($"/api/v1/forms/templates/{result.Id}", result);
        })
            .WithName("CreateProcessTemplate")
            .WithSummary("Create a process template")
            .RequirePermission(FormsPermissions.ProcessTemplates.Create)
            .WithDescription("Create a new process template (e.g. 'Weekly 1-1', 'Daily Sync'). The template starts with no published version — use PublishFormVersion to attach the first one.")
            .Produces<ProcessTemplateDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict);
    }
}
