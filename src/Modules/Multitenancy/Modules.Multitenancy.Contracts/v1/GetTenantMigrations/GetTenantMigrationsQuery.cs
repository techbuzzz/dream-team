using DreamTeam.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Multitenancy.Contracts.v1.GetTenantMigrations;

public sealed record GetTenantMigrationsQuery : IQuery<IReadOnlyCollection<TenantMigrationStatusDto>>;