using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.ArchiveProcessTemplate;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.ArchiveProcessTemplate;

public sealed class ArchiveProcessTemplateCommandHandler : ICommandHandler<ArchiveProcessTemplateCommand, ProcessTemplateDto>
{
    private readonly FormsDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ArchiveProcessTemplateCommandHandler(FormsDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<ProcessTemplateDto> Handle(ArchiveProcessTemplateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenantId = _currentUser.GetTenant();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new CustomException(
                "Request arrived without a resolved tenant. The Forms module requires a tenant context.",
                errors: null,
                System.Net.HttpStatusCode.BadRequest);
        }

        // Load tracked (we're about to mutate). Tenant filter narrows WHERE.
        var template = await _dbContext.ProcessTemplates
            .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Process template with ID '{command.Id}' not found.");

        // Domain method enforces the "archive only once" invariant. It throws
        // InvalidOperationException if already archived; we surface that as
        // 409 to match the Mark* state-transition endpoints.
        if (template.IsArchived)
        {
            throw new CustomException(
                $"Process template '{template.Id}' is already archived.",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        template.Archive();
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
