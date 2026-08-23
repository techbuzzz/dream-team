using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Framework.Storage.Services;
using DreamTeam.Modules.Files.Contracts;
using DreamTeam.Modules.Files.Contracts.v1.DTOs;
using DreamTeam.Modules.Files.Contracts.v1.Queries;
using DreamTeam.Modules.Files.Data;
using DreamTeam.Modules.Files.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DreamTeam.Modules.Files.Features.v1.GetFileDownloadUrl;

public sealed class GetFileDownloadUrlQueryHandler(
    FilesDbContext db,
    IStorageService storage,
    FileAccessPolicyRegistry policies,
    ICurrentUser currentUser,
    IOptions<FilesOptions> options)
    : IQueryHandler<GetFileDownloadUrlQuery, PresignedDownloadResponse>
{
    public async ValueTask<PresignedDownloadResponse> Handle(GetFileDownloadUrlQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);

        var f = await db.FileAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == q.FileAssetId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("file not found");

        var userId = currentUser.GetUserId().ToString();
        var policy = policies.Resolve(f.OwnerType)
            ?? throw new NotFoundException("file not found");

        var ctx = new FileAccessContext(f.Id, f.OwnerType, f.OwnerId, f.CreatedByUserId, (int)f.Visibility);
        if (!await policy.CanReadAsync(ctx, userId, cancellationToken).ConfigureAwait(false))
        {
            throw new NotFoundException("file not found");
        }

        var ttl = TimeSpan.FromMinutes(options.Value.DownloadUrlTtlMinutes);
        var mode = q.Inline ? "inline" : "attachment";
        var disposition = $"{mode}; filename=\"{f.OriginalFileName}\"";
        var url = await storage.GenerateDownloadUrlAsync(f.StorageKey, ttl, disposition, cancellationToken).ConfigureAwait(false);
        return new PresignedDownloadResponse(url, DateTimeOffset.UtcNow.Add(ttl));
    }
}
