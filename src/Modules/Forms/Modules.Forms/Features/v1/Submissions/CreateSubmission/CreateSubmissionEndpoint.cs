using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.Submissions.CreateSubmission;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.Submissions.CreateSubmission;

public static class CreateSubmissionEndpoint
{
    public static RouteHandlerBuilder MapCreateSubmissionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/instances/{instanceId:guid}/submissions", async (
                IMediator mediator,
                [FromRoute] Guid instanceId,
                [FromBody] CreateSubmissionRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateSubmissionCommand(
                    ProcessInstanceId: instanceId,
                    FormVersionId: request.FormVersionId,
                    Data: request.Data,
                    CompensatesSubmissionId: request.CompensatesSubmissionId);

                var result = await mediator.Send(command, cancellationToken);
                return TypedResults.Created(
                    $"/api/v1/forms/instances/{instanceId}/submissions/{result.Id}",
                    result);
            })
            .WithName("CreateSubmission")
            .WithSummary("Submit a response to a process instance")
            .RequirePermission(FormsPermissions.Submissions.Create)
            .WithDescription("Writes an immutable Submission row and atomically transitions the target ProcessInstance to Completed. Returns 404 if the instance does not exist, 409 if it is terminal or the FormVersionId does not match the instance's snapshot, 404 if CompensatesSubmissionId points at a non-existent row. Append-only: to amend a prior submission, send a new request with CompensatesSubmissionId set.")
            .Produces<SubmissionDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}

/// <summary>
/// Wire DTO for the endpoint. The instanceId comes from the route; the
/// rest is in the body. Kept separate from the command record so the
/// command's parameter list is the canonical request shape.
/// </summary>
public sealed record CreateSubmissionRequest(
    Guid FormVersionId,
    string Data,
    Guid? CompensatesSubmissionId = null);
