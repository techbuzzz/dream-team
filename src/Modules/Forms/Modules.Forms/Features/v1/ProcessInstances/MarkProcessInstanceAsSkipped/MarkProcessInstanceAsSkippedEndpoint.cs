using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.MarkProcessInstanceAsSkipped;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.MarkProcessInstanceAsSkipped;

public static class MarkProcessInstanceAsSkippedEndpoint
{
    public static RouteHandlerBuilder MapMarkProcessInstanceAsSkippedEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/instances/{instanceId:guid}/skip", async (
                IMediator mediator,
                [FromRoute] Guid instanceId,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new MarkProcessInstanceAsSkippedCommand(instanceId), cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("MarkProcessInstanceAsSkipped")
            .WithSummary("Mark a process instance as skipped")
            .RequirePermission(FormsPermissions.ProcessInstances.Skip)
            .WithDescription("Transitions a Planned/Running ProcessInstance to Skipped. Idempotent only by rejection — a second call on a terminal instance returns 409. CompletedAt is left null (the ritual did not happen).")
            .Produces<ProcessInstanceDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}
