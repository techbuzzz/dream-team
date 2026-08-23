using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.GetProcessTemplates;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.GetProcessTemplates;

public static class GetProcessTemplatesEndpoint
{
    public static RouteHandlerBuilder MapGetProcessTemplatesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/templates", async (
            IMediator mediator,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] string? sort,
            [FromQuery] string? searchTerm,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProcessTemplatesQuery(
                PageNumber: pageNumber,
                PageSize: pageSize,
                Sort: sort,
                SearchTerm: searchTerm);
            var result = await mediator.Send(query, cancellationToken);
            return TypedResults.Ok(result);
        })
            .WithName("GetProcessTemplates")
            .WithSummary("List process templates")
            .RequirePermission(FormsPermissions.ProcessTemplates.View)
            .WithDescription("Paginated list of process templates. Supports case-insensitive search on Name and Slug, and a small allowlist of Sort values (Name, NameDesc, CreatedOnUtc, CreatedOnUtcDesc).")
            .Produces<PagedResponse<object>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
