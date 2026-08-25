using Asp.Versioning;
using DreamTeam.Framework.Persistence;
using DreamTeam.Framework.Shared.Constants;
using DreamTeam.Framework.Web.Modules;
using DreamTeam.Modules.Forms.Contracts.Authorization;
using DreamTeam.Modules.Forms.Data;
using DreamTeam.Modules.Forms.Features.v1.FormVersions.CreateFormVersion;
using DreamTeam.Modules.Forms.Features.v1.FormVersions.GetCurrentFormVersion;
using DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionById;
using DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionsByTemplateId;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.CreateProcessInstance;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstanceById;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByTemplateId;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByUserId;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.MarkProcessInstanceAsCompleted;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.MarkProcessInstanceAsSkipped;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.UpdateProcessInstance;
using DreamTeam.Modules.Forms.Features.v1.Submissions.CreateSubmission;
using DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionById;
using DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionsByInstanceId;
using DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.ArchiveProcessTemplate;
using DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.CreateProcessTemplate;
using DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.GetProcessTemplateById;
using DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.GetProcessTemplates;
using DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.UpdateProcessTemplate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace DreamTeam.Modules.Forms;

/// <summary>
/// Forms module — the form engine per docs/architecture-v1.md §1-4. Owns
/// ProcessTemplate, FormVersion, ProcessInstance, Submission.
///
/// MVP-1 first vertical slice: CreateProcessTemplate. The Builder (E1.5),
/// Renderer (E1.4), FormVersion publish, ProcessInstance generation, and
/// Submission write land in follow-up workstreams against this module.
/// </summary>
public sealed class FormsModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(FormsPermissions.All);

        builder.Services.AddHeroDbContext<FormsDbContext>();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<FormsDbContext>(
                name: "db:forms",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No middleware. The Forms module has no request-scoped middleware in MVP-1.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/forms")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        // MVP-1 first vertical slice. Additional endpoints (publish form version,
        // generate process instance, submit response) land as follow-up workstreams.
        group.MapCreateProcessTemplateEndpoint();
        group.MapGetProcessTemplateByIdEndpoint();
        group.MapGetProcessTemplatesEndpoint();
        group.MapUpdateProcessTemplateEndpoint();
        group.MapArchiveProcessTemplateEndpoint();

        // E1.1 next slice: publish a FormVersion against a template (snapshot-on-publish).
        group.MapCreateFormVersionEndpoint();
        group.MapGetFormVersionByIdEndpoint();
        group.MapGetFormVersionsByTemplateIdEndpoint();
        group.MapGetCurrentFormVersionEndpoint();

        // E1.1 next slice: bridge to MVP-2 Rituals — schedule a ProcessInstance.
        group.MapCreateProcessInstanceEndpoint();
        group.MapGetProcessInstanceByIdEndpoint();

        // E1.1 next slice: state transition — mark instance Completed.
        group.MapMarkProcessInstanceAsCompletedEndpoint();

        // E1.1 next slice: state transition — mark instance Skipped.
        group.MapMarkProcessInstanceAsSkippedEndpoint();

        // E1.1 missed: list instances for a user (drives "My 1-1s" dashboard).
        group.MapGetProcessInstancesByUserIdEndpoint();

        // E1.1 missed: list instances for a template (paginated, JOIN through FormVersion).
        group.MapGetProcessInstancesByTemplateIdEndpoint();

        // E1.1 missed: PATCH an instance (reschedule / reassign pair). Terminal instances rejected.
        group.MapUpdateProcessInstanceEndpoint();

        // E1.1 keystone: submit a response to a ProcessInstance. Auto-transitions
        // the instance to Completed in the same transaction (the second path to
        // completion, alongside MarkProcessInstanceAsCompleted).
        group.MapCreateSubmissionEndpoint();
        group.MapGetSubmissionsByInstanceIdEndpoint();
        group.MapGetSubmissionByIdEndpoint();
    }
}
