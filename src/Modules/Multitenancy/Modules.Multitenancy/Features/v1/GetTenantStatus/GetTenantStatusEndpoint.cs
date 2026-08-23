using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Modules.Multitenancy.Contracts.Authorization;
using DreamTeam.Modules.Multitenancy.Contracts.Dtos;
using DreamTeam.Modules.Multitenancy.Contracts.v1.GetTenantStatus;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Multitenancy.Features.v1.GetTenantStatus;

public static class GetTenantStatusEndpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{id}/status", async (string id, IMediator mediator, CancellationToken cancellationToken) =>
                TypedResults.Ok(await mediator.Send(new GetTenantStatusQuery(id), cancellationToken)))
            .WithName("GetTenantStatus")
            .WithSummary("Get tenant status")
            .WithDescription("Retrieve status information for a tenant, including activation, validity, and basic metadata.")
            .RequirePermission(MultitenancyPermissions.Tenants.View)
            .Produces<TenantStatusDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}