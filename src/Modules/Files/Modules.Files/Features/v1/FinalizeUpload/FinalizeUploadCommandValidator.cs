using FluentValidation;
using DreamTeam.Modules.Files.Contracts.v1.Commands;

namespace DreamTeam.Modules.Files.Features.v1.FinalizeUpload;

public sealed class FinalizeUploadCommandValidator : AbstractValidator<FinalizeUploadCommand>
{
    public FinalizeUploadCommandValidator()
    {
        RuleFor(x => x.FileAssetId).NotEmpty();
    }
}
