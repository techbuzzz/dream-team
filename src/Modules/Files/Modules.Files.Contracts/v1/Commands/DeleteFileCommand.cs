using Mediator;

namespace DreamTeam.Modules.Files.Contracts.v1.Commands;

public sealed record DeleteFileCommand(Guid FileAssetId) : ICommand<Unit>;
