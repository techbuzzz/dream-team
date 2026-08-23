using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Modules.Multitenancy.Contracts.Authorization;
using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Multitenancy.Contracts.Dtos;
using DreamTeam.Modules.Multitenancy.Contracts.v1.GetTenants;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Multitenancy.Features.v1.GetTenants;

public static class GetTenantsEndpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet(
                "/",
                async ([AsParameters] GetTenantsQuery query, IMediator mediator, CancellationToken cancellationToken) =>
                    TypedResults.Ok(await mediator.Send(query, cancellationToken)))
            .WithName("ListTenants")
            .WithSummary("List tenants")
            .WithDescription("Retrieve tenants for the current environment with pagination and optional sorting.")
            .RequirePermission(MultitenancyPermissions.Tenants.View)
            .Produces<PagedResponse<TenantDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}