using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.Submissions.CreateSubmission;
using DreamTeam.Modules.Forms.Data;
using DreamTeam.Modules.Forms.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.Submissions.CreateSubmission;

public sealed class CreateSubmissionCommandHandler : ICommandHandler<CreateSubmissionCommand, SubmissionDto>
{
    private readonly FormsDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreateSubmissionCommandHandler(FormsDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<SubmissionDto> Handle(CreateSubmissionCommand command, CancellationToken cancellationToken)
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

        // Load the ProcessInstance (tenant-isolated). 404 if missing.
        var instance = await _dbContext.ProcessInstances
            .FirstOrDefaultAsync(i => i.Id == command.ProcessInstanceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Process instance with ID '{command.ProcessInstanceId}' not found.");

        // Terminal instances reject further submissions — append-only does
        // NOT mean "submit against a closed instance". Use the
        // MarkAsCompleted / MarkAsSkipped / reopen flows to change state.
        if (instance.Status is Domain.ProcessStatus.Completed or Domain.ProcessStatus.Skipped)
        {
            throw new CustomException(
                $"Process instance '{instance.Id}' is in terminal state '{instance.Status}'. " +
                "Submissions are not accepted against closed instances.",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        // Snapshot-on-publish invariant: the FormVersionId on the command
        // must match the instance's bound FormVersion. A mismatch means the
        // caller filled the wrong schema (e.g. a stale browser tab).
        if (instance.FormVersionId != command.FormVersionId)
        {
            throw new CustomException(
                $"FormVersionId mismatch: the ProcessInstance is bound to " +
                $"'{instance.FormVersionId}' but the submission references '{command.FormVersionId}'. " +
                "Reload the form against the live version and retry.",
                errors: null,
                System.Net.HttpStatusCode.Conflict);
        }

        // If CompensatesSubmissionId is set, validate that the target
        // submission actually exists in this tenant. A 404 surfaces as
        // "we don't recognise the prior submission" — preventing a caller
        // from appending a fake link to a non-existent row.
        if (command.CompensatesSubmissionId is { } priorId)
        {
            var priorExists = await _dbContext.Submissions
                .AsNoTracking()
                .AnyAsync(s => s.Id == priorId, cancellationToken)
                .ConfigureAwait(false);
            if (!priorExists)
            {
                throw new NotFoundException($"Prior submission '{priorId}' not found.");
            }
        }

        var authorId = _currentUser.GetUserId().ToString();
        var isCompensating = command.CompensatesSubmissionId is not null;

        var submission = Submission.Submit(
            processInstanceId: instance.Id,
            formVersionId: command.FormVersionId,
            tenantId: tenantId,
            authorId: authorId,
            data: command.Data,
            isCompensating: isCompensating,
            compensatesSubmissionId: command.CompensatesSubmissionId);

        _dbContext.Submissions.Add(submission);

        // Auto-transition the instance to Completed in the SAME SaveChanges
        // — atomic with the submission write. We do this via ExecuteUpdate
        // (sibling row) rather than mutate-instance-then-save because
        // ProcessInstance.Status is { get; private set; }. The SaveChanges
        // wraps the INSERT and the UPDATE in one transaction.
        await _dbContext.ProcessInstances
            .Where(i => i.Id == command.ProcessInstanceId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(i => i.Status, Domain.ProcessStatus.Completed)
                    .SetProperty(i => i.CompletedAt, (DateTime?)DateTime.UtcNow),
                cancellationToken)
            .ConfigureAwait(false);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new SubmissionDto(
            Id: submission.Id,
            ProcessInstanceId: submission.ProcessInstanceId,
            FormVersionId: submission.FormVersionId,
            AuthorId: submission.AuthorId,
            Data: submission.Data,
            IsCompensating: submission.IsCompensating,
            CompensatesSubmissionId: submission.CompensatesSubmissionId,
            CreatedOnUtc: submission.CreatedOnUtc);
    }
}
