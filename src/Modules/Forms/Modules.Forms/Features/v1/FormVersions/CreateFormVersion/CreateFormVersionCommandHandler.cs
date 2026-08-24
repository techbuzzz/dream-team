using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.CreateFormVersion;
using DreamTeam.Modules.Forms.Data;
using DreamTeam.Modules.Forms.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.CreateFormVersion;

public sealed class CreateFormVersionCommandHandler : ICommandHandler<CreateFormVersionCommand, FormVersionDto>
{
    private readonly FormsDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreateFormVersionCommandHandler(FormsDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<FormVersionDto> Handle(CreateFormVersionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenantId = _currentUser.GetTenant();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            // Same rationale as CreateProcessTemplate: a missing tenant is a 400,
            // not a 201. Single-tenant MVP-1 but the request still needs a
            // resolved tenant header.
            throw new CustomException(
                "Request arrived without a resolved tenant. The Forms module requires a tenant context.",
                errors: null,
                System.Net.HttpStatusCode.BadRequest);
        }

        // Load template (tenant filter applies). 404 if missing/soft-deleted.
        var template = await _dbContext.ProcessTemplates
            .FirstOrDefaultAsync(t => t.Id == command.ProcessTemplateId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Process template with ID '{command.ProcessTemplateId}' not found.");

        if (template.IsArchived)
        {
            // Archiving is a soft-state — the template exists but should not
            // accept new versions. The lead should either un-archive it (future
            // endpoint) or create a new template.
            throw new CustomException(
                $"Process template '{template.Id}' is archived. Un-archive it or create a new template to publish further versions.",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        var publishedById = _currentUser.GetUserId().ToString();

        // Compute the next version number. MAX() on an empty set returns NULL;
        // coalesce to 0, then +1 → first version is 1.
        var maxVersion = await _dbContext.FormVersions
            .Where(v => v.ProcessTemplateId == command.ProcessTemplateId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false);
        var nextVersionNumber = (maxVersion ?? 0) + 1;

        // Flip the previous current version to non-current. ExecuteUpdateAsync
        // bypasses the entity model (private setters don't matter) and writes
        // a single SQL UPDATE — efficient and avoids loading every old version
        // into the change tracker. The base DbContext's tenant-isolation
        // query filter narrows the UPDATE to this tenant automatically.
        await _dbContext.FormVersions
            .Where(v => v.ProcessTemplateId == command.ProcessTemplateId && v.IsCurrent)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(v => v.IsCurrent, false),
                cancellationToken)
            .ConfigureAwait(false);

        // Snapshot-on-publish: the new FormVersion is immutable from here on.
        // The factory sets IsCurrent = true and PublishedAt = UtcNow.
        var version = FormVersion.Publish(
            processTemplateId: command.ProcessTemplateId,
            tenantId: tenantId,
            versionNumber: nextVersionNumber,
            schema: command.Schema,
            description: command.Description,
            publishedById: publishedById);

        _dbContext.FormVersions.Add(version);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new FormVersionDto(
            Id: version.Id,
            ProcessTemplateId: version.ProcessTemplateId,
            VersionNumber: version.VersionNumber,
            Description: version.Description,
            IsCurrent: version.IsCurrent,
            PublishedById: version.PublishedById,
            PublishedAt: version.PublishedAt);
    }
}
