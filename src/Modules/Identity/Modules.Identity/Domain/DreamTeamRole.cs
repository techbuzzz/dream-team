using Microsoft.AspNetCore.Identity;

namespace DreamTeam.Modules.Identity.Domain;

public class DreamTeamRole : IdentityRole
{
    public string? Description { get; set; }

    public DreamTeamRole(string name, string? description = null)
        : base(name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Description = description;
        NormalizedName = name.ToUpperInvariant();
    }
}