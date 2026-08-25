using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.UpdateProcessTemplate;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.UpdateProcessTemplate;

public sealed class UpdateProcessTemplateCommandHandler : ICommandHandler<UpdateProcessTemplateCommand, ProcessTemplateDto>
{
    private readonly FormsDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public UpdateProcessTemplateCommandHandler(FormsDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<ProcessTemplateDto> Handle(UpdateProcessTemplateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Tenant context required even for an UPDATE — the entity write
        // goes through the tenant-isolation filter and the audit
        // interceptor needs a tenant to stamp.
        var tenantId = _currentUser.GetTenant();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new CustomException(
                "Request arrived without a resolved tenant. The Forms module requires a tenant context.",
                errors: null,
                System.Net.HttpStatusCode.BadRequest);
        }

        // Load tracked (we're about to mutate). Tenant-isolation filter
        // narrows the WHERE; an Id in another tenant surfaces as 404.
        var template = await _dbContext.ProcessTemplates
            .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Process template with ID '{command.Id}' not found.");

        if (template.IsArchived)
        {
            throw new CustomException(
                $"Process template '{template.Id}' is archived. Un-archive it before mutating.",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        // Domain method enforces the per-field guards (non-empty when present,
        // null = leave unchanged). Slug and OwnerId are intentionally not
        // touchable from this path.
        template.Update(
            name: command.Name,
            description: command.Description,
            category: command.Category);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
