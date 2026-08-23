using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Framework.Storage.Services;
using DreamTeam.Modules.Files.Contracts;
using DreamTeam.Modules.Files.Contracts.v1.DTOs;
using DreamTeam.Modules.Files.Contracts.v1.Queries;
using DreamTeam.Modules.Files.Data;
using DreamTeam.Modules.Files.Domain;
using DreamTeam.Modules.Files.Features.v1.Internal;
using DreamTeam.Modules.Files.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Files.Features.v1.GetFileMetadata;

public sealed class GetFileMetadataQueryHandler(
    FilesDbContext db,
    FileAccessPolicyRegistry policies,
    ICurrentUser currentUser,
    IStorageService storage)
    : IQueryHandler<GetFileMetadataQuery, FileAssetDto>
{
    public async ValueTask<FileAssetDto> Handle(GetFileMetadataQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);

        var f = await db.FileAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == q.FileAssetId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("file not found");

        var userId = currentUser.GetUserId().ToString();
        var policy = policies.Resolve(f.OwnerType)
            ?? throw new NotFoundException("file not found"); // don't leak existence on missing policy

        var ctx = new FileAccessContext(f.Id, f.OwnerType, f.OwnerId, f.CreatedByUserId, (int)f.Visibility);
        if (!await policy.CanReadAsync(ctx, userId, cancellationToken).ConfigureAwait(false))
        {
            throw new NotFoundException("file not found");
        }

        // Public files get a durable URL safe to persist long-term, while private files mint a
        // short-lived presigned GET on demand via the auth-gated url endpoint.
        var publicUrl = f.Visibility == Visibility.Public
            ? storage.BuildPublicUrl(f.StorageKey)
            : null;

        return FileAssetMapper.ToDto(f, publicUrl);
    }
}
