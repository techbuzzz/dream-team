using FluentValidation;
using DreamTeam.Modules.Files.Contracts.v1.Commands;

namespace DreamTeam.Modules.Files.Features.v1.DeleteFile;

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.FileAssetId).NotEmpty();
    }
}
