namespace Integration.Tests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class DreamTeamCollectionDefinition : ICollectionFixture<DreamTeamWebApplicationFactory>
{
    public const string Name = "DreamTeamIntegration";
}
