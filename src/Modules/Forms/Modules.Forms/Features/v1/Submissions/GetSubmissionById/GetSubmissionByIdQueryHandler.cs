using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Modules.Forms.Contracts.Dtos;
using DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionById;
using DreamTeam.Modules.Forms.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionById;

public sealed class GetSubmissionByIdQueryHandler : IQueryHandler<GetSubmissionByIdQuery, SubmissionDto>
{
    private readonly FormsDbContext _dbContext;

    public GetSubmissionByIdQueryHandler(FormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<SubmissionDto> Handle(GetSubmissionByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // AsNoTracking: read-only projection. Tenant-isolation is automatic.
        var submission = await _dbContext.Submissions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Submission with ID '{query.Id}' not found.");

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
