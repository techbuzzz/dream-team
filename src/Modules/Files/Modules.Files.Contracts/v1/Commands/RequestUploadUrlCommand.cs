using DreamTeam.Modules.Files.Contracts.v1.DTOs;
using Mediator;

namespace DreamTeam.Modules.Files.Contracts.v1.Commands;

public sealed record RequestUploadUrlCommand(
    string OwnerType,
    Guid? OwnerId,
    string FileName,
    string ContentType,
    long SizeBytes,
    Visibility Visibility,
    string Category) : ICommand<PresignedUploadResponse>;
