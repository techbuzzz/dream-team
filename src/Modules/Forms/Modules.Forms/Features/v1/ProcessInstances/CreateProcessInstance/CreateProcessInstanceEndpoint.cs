using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.CreateProcessInstance;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.CreateProcessInstance;

public static class CreateProcessInstanceEndpoint
{
    public static RouteHandlerBuilder MapCreateProcessInstanceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/instances", async (
                IMediator mediator,
                [FromBody] CreateProcessInstanceRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateProcessInstanceCommand(
                    FormVersionId: request.FormVersionId,
                    ScheduledAt: request.ScheduledAt,
                    PairUserId: request.PairUserId);

                var result = await mediator.Send(command, cancellationToken);
                return TypedResults.Created(
                    $"/api/v1/forms/instances/{result.Id}",
                    result);
            })
            .WithName("CreateProcessInstance")
            .WithSummary("Schedule a process instance against a published form version")
            .RequirePermission(FormsPermissions.ProcessInstances.View)   // View gate: only authorized users schedule
            .WithDescription("Creates a Planned ProcessInstance. The FormVersion must exist (404) and be the current published version (409 otherwise). PairUserId is required for 1-1s and null for whole-team rituals (Daily, Retro).")
            .Produces<ProcessInstanceDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}

/// <summary>
/// Wire DTO for the endpoint. Kept separate from the command so the command
/// record's parameter list is the canonical request shape (no envelope).
/// </summary>
public sealed record CreateProcessInstanceRequest(
    Guid FormVersionId,
    DateTime ScheduledAt,
    string? PairUserId = null);
