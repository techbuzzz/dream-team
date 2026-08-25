using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.UpdateProcessInstance;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.UpdateProcessInstance;

public static class UpdateProcessInstanceEndpoint
{
    public static RouteHandlerBuilder MapUpdateProcessInstanceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/instances/{instanceId:guid}", async (
                IMediator mediator,
                [FromRoute] Guid instanceId,
                [FromBody] UpdateProcessInstanceRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateProcessInstanceCommand(
                    Id: instanceId,
                    ScheduledAt: request.ScheduledAt,
                    PairUserId: request.PairUserId);

                var result = await mediator.Send(command, cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("UpdateProcessInstance")
            .WithSummary("Update a process instance (PATCH — ScheduledAt, PairUserId)")
            .RequirePermission(FormsPermissions.ProcessInstances.View)   // MVP-1: View gate; tighten in E1.2 (RBAC)
            .WithDescription("PATCH-style content update. Both fields optional; null = leave unchanged. FormVersionId is NOT mutable from this endpoint. Returns 400 if both fields are null, 404 if the instance does not exist, 409 if it is in a terminal state (Completed/Skipped).")
            .Produces<ProcessInstanceDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}

/// <summary>
/// Wire DTO. Both fields optional for PATCH semantics. JSON deserializer
/// maps omitted fields to null (which the domain method treats as
/// "leave unchanged" per the contract).
/// </summary>
public sealed record UpdateProcessInstanceRequest(
    DateTime? ScheduledAt,
    string? PairUserId);
