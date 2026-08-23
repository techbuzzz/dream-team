using FluentValidation;
using DreamTeam.Modules.Files.Contracts.v1.Queries;

namespace DreamTeam.Modules.Files.Features.v1.ListSharedFiles;

public sealed class ListSharedFilesQueryValidator : AbstractValidator<ListSharedFilesQuery>
{
    public ListSharedFilesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
