using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Identity.Contracts.Authorization;
using DreamTeam.Modules.Identity.Contracts.DTOs;
using DreamTeam.Modules.Identity.Contracts.v1.Sessions.GetTenantSessions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Identity.Features.v1.Sessions.GetTenantSessions;

public static class GetTenantSessionsEndpoint
{
    internal static RouteHandlerBuilder MapGetTenantSessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/sessions",
                async (
                    bool? includeInactive,
                    string? search,
                    int? pageNumber,
                    int? pageSize,
                    IMediator mediator,
                    CancellationToken ct) =>
                {
                    var query = new GetTenantSessionsQuery
                    {
                        IncludeInactive = includeInactive ?? false,
                        Search = search,
                        PageNumber = pageNumber ?? 1,
                        PageSize = pageSize ?? 50,
                    };
                    return TypedResults.Ok(await mediator.Send(query, ct));
                })
            .WithName("GetTenantSessions")
            .WithSummary("List all sessions in the current tenant (Admin)")
            .WithDescription("Returns paged sessions across the tenant, filterable by active state and a free-text search across user name, email, and IP address.")
            .RequirePermission(IdentityPermissions.Sessions.ViewAll)
            .Produces<PagedResponse<UserSessionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
