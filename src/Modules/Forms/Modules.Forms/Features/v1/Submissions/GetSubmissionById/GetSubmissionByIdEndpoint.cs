using DreamTeam.Framework.Shared.Identity.Authorization;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionById;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionById;

public static class GetSubmissionByIdEndpoint
{
    public static RouteHandlerBuilder MapGetSubmissionByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/submissions/{submissionId:guid}", async (
                IMediator mediator,
                [FromRoute] Guid submissionId,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetSubmissionByIdQuery(submissionId), cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("GetSubmissionById")
            .WithSummary("Get a submission by its ID")
            .RequirePermission(FormsPermissions.Submissions.View)
            .WithDescription("Returns the Submission identified by `submissionId`. 404 if it does not exist (or belongs to another tenant).")
            .Produces<SubmissionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
