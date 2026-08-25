using DreamTeam.Modules.Forms.Domain;
using Shouldly;
using Xunit;

namespace Forms.Tests.Domain;

/// <summary>
/// Domain tests for the ProcessTemplate entity. Pure unit tests — no
/// DbContext, no ICurrentUser. They pin the factory's invariants so future
/// refactors can't accidentally break the create-time contract.
/// </summary>
public sealed class ProcessTemplateTests
{
    [Fact]
    public void Create_Should_SetAllRequiredFields()
    {
        // Arrange
        var tenantId = "tenant-1";
        var name = "Weekly 1-1";
        var slug = "weekly-1-1";
        var description = "Lead and member sync";
        var ownerId = "user-42";
        var category = "ONE_ON_ONE";

        // Act
        var template = ProcessTemplate.Create(tenantId, name, slug, description, ownerId, category);

        // Assert
        template.Id.ShouldNotBe(Guid.Empty);
        template.TenantId.ShouldBe(tenantId);
        template.Name.ShouldBe(name);
        template.Slug.ShouldBe(slug);
        template.Description.ShouldBe(description);
        template.OwnerId.ShouldBe(ownerId);
        template.Category.ShouldBe(category);
        template.IsArchived.ShouldBeFalse();
        template.IsDeleted.ShouldBeFalse();
        template.DeletedOnUtc.ShouldBeNull();
        template.DeletedBy.ShouldBeNull();
        template.CreatedOnUtc.ShouldBe(default);   // populated by interceptor at SaveChanges, not in factory
        template.LastModifiedOnUtc.ShouldBeNull();
    }

    [Fact]
    public void Create_Should_Throw_When_TenantIdIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            ProcessTemplate.Create(
                tenantId: "",
                name: "Test",
                slug: "test",
                description: null,
                ownerId: "user-1",
                category: null));
    }

    [Fact]
    public void Create_Should_Throw_When_NameIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            ProcessTemplate.Create(
                tenantId: "tenant-1",
                name: "",
                slug: "test",
                description: null,
                ownerId: "user-1",
                category: null));
    }

    [Fact]
    public void Create_Should_Throw_When_SlugIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            ProcessTemplate.Create(
                tenantId: "tenant-1",
                name: "Test",
                slug: "",
                description: null,
                ownerId: "user-1",
                category: null));
    }

    [Fact]
    public void Create_Should_Throw_When_OwnerIdIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            ProcessTemplate.Create(
                tenantId: "tenant-1",
                name: "Test",
                slug: "test",
                description: null,
                ownerId: "",
                category: null));
    }
}
