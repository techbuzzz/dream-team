using FluentValidation;
using DreamTeam.Modules.Multitenancy.Contracts.v1.ChangeTenantActivation;

namespace DreamTeam.Modules.Multitenancy.Features.v1.ChangeTenantActivation;

internal sealed class ChangeTenantActivationCommandValidator : AbstractValidator<ChangeTenantActivationCommand>
{
    public ChangeTenantActivationCommandValidator() =>
       RuleFor(t => t.TenantId)
           .NotEmpty();
}