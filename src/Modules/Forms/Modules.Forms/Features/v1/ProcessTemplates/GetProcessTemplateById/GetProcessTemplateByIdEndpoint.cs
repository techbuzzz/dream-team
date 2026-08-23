using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.GetProcessTemplateById;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.GetProcessTemplateById;

public static class GetProcessTemplateByIdEndpoint
{
    public static RouteHandlerBuilder MapGetProcessTemplateByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/templates/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetProcessTemplateByIdQuery(id), cancellationToken);
            return TypedResults.Ok(result);
        })
            .WithName("GetProcessTemplateById")
            .WithSummary("Get process template by ID")
            .RequirePermission(FormsPermissions.ProcessTemplates.View)
            .WithDescription("Retrieve a specific process template by its ID. Returns 404 if the template does not exist or belongs to a different tenant.")
            .Produces<ProcessTemplateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
