using System.Collections.ObjectModel;
using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Core.Exceptions;
using DreamTeam.Framework.Storage.Services;
using DreamTeam.Modules.Files.Contracts.v1.DTOs;
using DreamTeam.Modules.Files.Contracts.v1.Queries;
using DreamTeam.Modules.Files.Data;
using DreamTeam.Modules.Files.Domain;
using DreamTeam.Modules.Files.Features.v1.Internal;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Modules.Files.Features.v1.ListMyFiles;

public sealed class ListMyFilesQueryHandler(
    FilesDbContext db,
    ICurrentUser currentUser,
    IStorageService storage)
    : IQueryHandler<ListMyFilesQuery, ReadOnlyCollection<FileAssetDto>>
{
    public async ValueTask<ReadOnlyCollection<FileAssetDto>> Handle(ListMyFilesQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);
        var userId = currentUser.GetUserId().ToString();
        if (string.IsNullOrEmpty(userId) || userId == Guid.Empty.ToString())
        {
            throw new UnauthorizedException("no current user");
        }

        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);

        var rows = await db.FileAssets.AsNoTracking()
            .Where(f => f.CreatedByUserId == userId && f.Status == FileAssetStatus.Available)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Seed publicUrl for public files so the preview dialog can paint the image
        // immediately from the list data, without waiting on a metadata refetch to mint it.
        return rows
            .Select(f => FileAssetMapper.ToDto(
                f,
                f.Visibility == Visibility.Public ? storage.BuildPublicUrl(f.StorageKey) : null))
            .ToList()
            .AsReadOnly();
    }
}
