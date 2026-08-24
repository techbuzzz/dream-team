using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Framework.Shared.Persistence;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionsByTemplateId;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionsByTemplateId;

public static class GetFormVersionsByTemplateIdEndpoint
{
    public static RouteHandlerBuilder MapGetFormVersionsByTemplateIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/templates/{templateId:guid}/versions", async (
                IMediator mediator,
                [FromRoute] Guid templateId,
                [FromQuery] bool? isCurrent,
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize,
                [FromQuery] string? sort,
                CancellationToken cancellationToken) =>
            {
                var query = new GetFormVersionsByTemplateIdQuery(
                    TemplateId: templateId,
                    IsCurrent: isCurrent,
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                    Sort: sort);
                var result = await mediator.Send(query, cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("GetFormVersionsByTemplateId")
            .WithSummary("List form versions for a process template")
            .RequirePermission(FormsPermissions.FormVersions.View)
            .WithDescription("Paginated list of FormVersions for the given template. Optional `isCurrent` filter restricts to the single live version. Sort allowlist: VersionNumber, VersionNumberDesc, PublishedAt, PublishedAtDesc.")
            .Produces<PagedResponse<object>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
