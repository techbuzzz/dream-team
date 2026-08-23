using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.CreateProcessTemplate;
using DreamTeam.Modules.Forms.Data;
using DreamTeam.Modules.Forms.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.CreateProcessTemplate;

public sealed class CreateProcessTemplateCommandHandler : ICommandHandler<CreateProcessTemplateCommand, ProcessTemplateDto>
{
    private readonly FormsDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreateProcessTemplateCommandHandler(FormsDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<ProcessTemplateDto> Handle(CreateProcessTemplateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenantId = _currentUser.GetTenant();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            // MVP-1 is single-tenant but a missing tenant (e.g. the request
            // arrived without a Finbuckle-resolved tenant) is a 500, not a 201.
            // The host's UseHeroMultiTenantDatabases() middleware is OFF in
            // MVP-1 (per the FSH-strip prep), so this only fires for operators
            // hitting the host without a tenant header.
            throw new CustomException(
                "Request arrived without a resolved tenant. The Forms module requires a tenant context.",
                errors: null,
                System.Net.HttpStatusCode.BadRequest);
        }

        var ownerId = _currentUser.GetUserId().ToString();

        // Slug uniqueness is enforced by a unique index (TenantId, Slug); a
        // race-safe pre-check gives a clean 409 instead of a 500 from
        // Postgres unique_violation.
        var slugTaken = await _dbContext.ProcessTemplates
            .AnyAsync(t => t.Slug == command.Slug, cancellationToken)
            .ConfigureAwait(false);

        if (slugTaken)
        {
            throw new CustomException(
                $"A process template with slug '{command.Slug}' already exists.",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        var template = ProcessTemplate.Create(
            tenantId: tenantId,
            name: command.Name,
            slug: command.Slug,
            description: command.Description,
            ownerId: ownerId,
            category: command.Category);

        _dbContext.ProcessTemplates.Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // CreatedOnUtc is populated by the AuditableEntitySaveChangesInterceptor
        // before SaveChanges, so it's set on the entity by the time we get here.
        return new ProcessTemplateDto(
            Id: template.Id,
            Name: template.Name,
            Slug: template.Slug,
            Description: template.Description,
            OwnerId: template.OwnerId,
            Category: template.Category,
            IsArchived: template.IsArchived,
            CreatedOnUtc: template.CreatedOnUtc);
    }
}
