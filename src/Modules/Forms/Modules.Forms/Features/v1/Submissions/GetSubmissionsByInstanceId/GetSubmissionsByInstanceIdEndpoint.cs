using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionsByInstanceId;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionsByInstanceId;

public static class GetSubmissionsByInstanceIdEndpoint
{
    public static RouteHandlerBuilder MapGetSubmissionsByInstanceIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/instances/{instanceId:guid}/submissions", async (
                IMediator mediator,
                [FromRoute] Guid instanceId,
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize,
                [FromQuery] string? sort,
                CancellationToken cancellationToken) =>
            {
                var query = new GetSubmissionsByInstanceIdQuery(
                    InstanceId: instanceId,
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                    Sort: sort);
                var result = await mediator.Send(query, cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("GetSubmissionsByInstanceId")
            .WithSummary("List submissions for a process instance")
            .RequirePermission(FormsPermissions.Submissions.View)
            .WithDescription("Paginated list of all Submissions (original + compensating corrections) for the instance, oldest-first. Sort allowlist: CreatedOnUtc, CreatedOnUtcDesc.")
            .Produces<PagedResponse<object>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
