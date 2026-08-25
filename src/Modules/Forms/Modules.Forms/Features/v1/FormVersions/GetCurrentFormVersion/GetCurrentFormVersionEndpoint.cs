using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetCurrentFormVersion;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.GetCurrentFormVersion;

public static class GetCurrentFormVersionEndpoint
{
    public static RouteHandlerBuilder MapGetCurrentFormVersionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/templates/{templateId:guid}/versions/current", async (
                IMediator mediator,
                [FromRoute] Guid templateId,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetCurrentFormVersionQuery(templateId), cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("GetCurrentFormVersion")
            .WithSummary("Get the currently-live form version for a template")
            .RequirePermission(FormsPermissions.FormVersions.View)
            .WithDescription("Returns the FormVersion with IsCurrent=true for the given template. 404 if the template does not exist or no version has been published yet.")
            .Produces<FormVersionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
