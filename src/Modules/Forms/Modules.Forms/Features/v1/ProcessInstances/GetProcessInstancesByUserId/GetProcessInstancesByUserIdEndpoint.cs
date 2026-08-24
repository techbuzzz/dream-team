using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByUserId;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByUserId;

public static class GetProcessInstancesByUserIdEndpoint
{
    public static RouteHandlerBuilder MapGetProcessInstancesByUserIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/instances", async (
                IMediator mediator,
                [FromQuery] string? pairUserId,
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize,
                [FromQuery] string? sort,
                CancellationToken cancellationToken) =>
            {
                var query = new GetProcessInstancesByUserIdQuery(
                    UserId: pairUserId ?? string.Empty,
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                    Sort: sort);
                var result = await mediator.Send(query, cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("GetProcessInstancesByUserId")
            .WithSummary("List process instances for a specific user (1-1 view)")
            .RequirePermission(FormsPermissions.ProcessInstances.View)
            .WithDescription("Paginated list of ProcessInstances where PairUserId = `pairUserId`. Sort allowlist: ScheduledAt (default, soonest first), ScheduledAtDesc, CreatedOnUtc, CreatedOnUtcDesc.")
            .Produces<PagedResponse<object>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
