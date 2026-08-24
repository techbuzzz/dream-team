using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstanceById;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstanceById;

public static class GetProcessInstanceByIdEndpoint
{
    public static RouteHandlerBuilder MapGetProcessInstanceByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/instances/{instanceId:guid}", async (
                IMediator mediator,
                [FromRoute] Guid instanceId,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetProcessInstanceByIdQuery(instanceId), cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("GetProcessInstanceById")
            .WithSummary("Get a process instance by its ID")
            .RequirePermission(FormsPermissions.ProcessInstances.View)
            .WithDescription("Returns the ProcessInstance identified by `instanceId`. 404 if it does not exist (or belongs to another tenant).")
            .Produces<ProcessInstanceDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
