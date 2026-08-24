using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionById;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionById;

public static class GetFormVersionByIdEndpoint
{
    public static RouteHandlerBuilder MapGetFormVersionByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/versions/{versionId:guid}", async (
                IMediator mediator,
                [FromRoute] Guid versionId,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetFormVersionByIdQuery(versionId), cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("GetFormVersionById")
            .WithSummary("Get a form version by its ID")
            .RequirePermission(FormsPermissions.FormVersions.View)
            .WithDescription("Returns the FormVersion identified by `versionId`. 404 if it does not exist (or belongs to another tenant).")
            .Produces<FormVersionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
