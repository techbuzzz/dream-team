using DreamTeam.Modules.Identity.Contracts.Authorization;
using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Identity.Contracts.DTOs;
using DreamTeam.Modules.Identity.Contracts.v1.Groups.GetGroups;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Identity.Features.v1.Groups.GetGroups;

public static class GetGroupsEndpoint
{
    public static RouteHandlerBuilder MapGetGroupsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/groups", async (IMediator mediator, string? search, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetGroupsQuery(search), cancellationToken)))
        .WithName("ListGroups")
        .WithSummary("List all groups")
        .RequirePermission(IdentityPermissions.Groups.View)
        .WithDescription("Retrieve all groups for the current tenant with optional search filter.")
        .Produces<IEnumerable<GroupDto>>(StatusCodes.Status200OK);
    }
}