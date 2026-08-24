using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.MarkProcessInstanceAsCompleted;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.MarkProcessInstanceAsCompleted;

public static class MarkProcessInstanceAsCompletedEndpoint
{
    public static RouteHandlerBuilder MapMarkProcessInstanceAsCompletedEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/instances/{instanceId:guid}/complete", async (
                IMediator mediator,
                [FromRoute] Guid instanceId,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new MarkProcessInstanceAsCompletedCommand(instanceId), cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("MarkProcessInstanceAsCompleted")
            .WithSummary("Mark a process instance as completed")
            .RequirePermission(FormsPermissions.ProcessInstances.View)   // MVP-1: View gate; tighten in E1.2 (RBAC)
            .WithDescription("Transitions a Planned/Running ProcessInstance to Completed. Idempotent only by rejection — a second call on a terminal instance returns 409. POST is used because this is an action endpoint, not a resource update.")
            .Produces<ProcessInstanceDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}
