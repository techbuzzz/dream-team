using DreamTeam.Modules.Forms.Domain;
using Shouldly;
using Xunit;

namespace Forms.Tests.Domain;

/// <summary>
/// Domain tests for the FormVersion entity. Pins the snapshot-on-publish
/// invariants: every freshly-published version starts with
/// <c>IsCurrent = true</c> and a non-default <c>PublishedAt</c>.
/// </summary>
public sealed class FormVersionTests
{
    [Fact]
    public void Publish_Should_StartAsCurrent_WithPublishedAtNow()
    {
        // Arrange
        var processTemplateId = Guid.NewGuid();
        var tenantId = "tenant-1";
        var schema = """{"id":"form_v_1","title":"1-1","version":1,"pages":[]}""";
        var publishedById = "user-42";

        // Act
        var before = DateTime.UtcNow;
        var version = FormVersion.Publish(
            processTemplateId: processTemplateId,
            tenantId: tenantId,
            versionNumber: 1,
            schema: schema,
            description: "first version",
            publishedById: publishedById);
        var after = DateTime.UtcNow;

        // Assert
        version.Id.ShouldNotBe(Guid.Empty);
        version.ProcessTemplateId.ShouldBe(processTemplateId);
        version.TenantId.ShouldBe(tenantId);
        version.VersionNumber.ShouldBe(1);
        version.Schema.ShouldBe(schema);
        version.Description.ShouldBe("first version");
        version.PublishedById.ShouldBe(publishedById);
        version.IsCurrent.ShouldBeTrue();
        version.PublishedAt.ShouldBeInRange(before, after);
    }

    [Fact]
    public void Publish_Should_Throw_When_TenantIdIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            FormVersion.Publish(
                processTemplateId: Guid.NewGuid(),
                tenantId: "",
                versionNumber: 1,
                schema: "{}",
                description: null,
                publishedById: "user-1"));
    }

    [Fact]
    public void Publish_Should_Throw_When_SchemaIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            FormVersion.Publish(
                processTemplateId: Guid.NewGuid(),
                tenantId: "tenant-1",
                versionNumber: 1,
                schema: "",
                description: null,
                publishedById: "user-1"));
    }

    [Fact]
    public void Publish_Should_Throw_When_PublishedByIdIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            FormVersion.Publish(
                processTemplateId: Guid.NewGuid(),
                tenantId: "tenant-1",
                versionNumber: 1,
                schema: "{}",
                description: null,
                publishedById: ""));
    }

    [Fact]
    public void Publish_Should_Throw_When_VersionNumberIsNotPositive()
    {
        // Act + Assert
        Should.Throw<ArgumentOutOfRangeException>(() =>
            FormVersion.Publish(
                processTemplateId: Guid.NewGuid(),
                tenantId: "tenant-1",
                versionNumber: 0,
                schema: "{}",
                description: null,
                publishedById: "user-1"));
    }
}
