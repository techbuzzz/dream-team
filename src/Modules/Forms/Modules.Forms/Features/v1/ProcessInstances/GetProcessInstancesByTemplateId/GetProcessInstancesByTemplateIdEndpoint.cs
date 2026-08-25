using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByTemplateId;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByTemplateId;

public static class GetProcessInstancesByTemplateIdEndpoint
{
    public static RouteHandlerBuilder MapGetProcessInstancesByTemplateIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/templates/{templateId:guid}/instances", async (
                IMediator mediator,
                [FromRoute] Guid templateId,
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize,
                [FromQuery] string? sort,
                CancellationToken cancellationToken) =>
            {
                var query = new GetProcessInstancesByTemplateIdQuery(
                    TemplateId: templateId,
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                    Sort: sort);
                var result = await mediator.Send(query, cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("GetProcessInstancesByTemplateId")
            .WithSummary("List process instances for a template (paginated)")
            .RequirePermission(FormsPermissions.ProcessInstances.View)
            .WithDescription("Paginated list of ProcessInstances pointing at any FormVersion of the given template. Sort allowlist: ScheduledAt (default, soonest first), ScheduledAtDesc, CreatedOnUtc, CreatedOnUtcDesc.")
            .Produces<PagedResponse<object>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
