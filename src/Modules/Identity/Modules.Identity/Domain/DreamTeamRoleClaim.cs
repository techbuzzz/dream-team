using Microsoft.AspNetCore.Identity;

namespace DreamTeam.Modules.Identity.Domain;

public class DreamTeamRoleClaim : IdentityRoleClaim<string>
{
    public string? CreatedBy { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}