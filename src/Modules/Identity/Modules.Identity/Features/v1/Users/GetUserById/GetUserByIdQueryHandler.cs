using DreamTeam.Modules.Identity.Contracts.DTOs;
using DreamTeam.Modules.Identity.Contracts.Services;
using DreamTeam.Modules.Identity.Contracts.v1.Users.GetUser;
using Mediator;

namespace DreamTeam.Modules.Identity.Features.v1.Users.GetUserById;

public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    private readonly IUserService _userService;

    public GetUserByIdQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<UserDto> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await _userService.GetAsync(query.Id, cancellationToken).ConfigureAwait(false);
    }
}