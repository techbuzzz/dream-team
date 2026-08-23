using FluentValidation;
using DreamTeam.Framework.Web.Validation;
using DreamTeam.Modules.Multitenancy.Contracts.v1.GetTenants;

namespace DreamTeam.Modules.Multitenancy.Features.v1.GetTenants;

public sealed class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsQueryValidator()
    {
        Include(new PagedQueryValidator<GetTenantsQuery>());
    }
}