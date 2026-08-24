using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionsByTemplateId;
using DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionsByTemplateId;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class GetFormVersionsByTemplateIdQueryValidatorTests
{
    private readonly GetFormVersionsByTemplateIdQueryValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForValidQuery()
    {
        // Arrange
        var query = new GetFormVersionsByTemplateIdQuery(
            TemplateId: Guid.NewGuid(),
            IsCurrent: null,
            PageNumber: 1,
            PageSize: 20,
            Sort: "VersionNumber");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_TemplateIdIsEmpty()
    {
        // Arrange
        var query = new GetFormVersionsByTemplateIdQuery(TemplateId: Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetFormVersionsByTemplateIdQuery.TemplateId));
    }

    [Fact]
    public void Validate_Should_Allow_IsCurrentTrue()
    {
        // Arrange
        var query = new GetFormVersionsByTemplateIdQuery(
            TemplateId: Guid.NewGuid(),
            IsCurrent: true);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Allow_IsCurrentFalse()
    {
        // Arrange
        var query = new GetFormVersionsByTemplateIdQuery(
            TemplateId: Guid.NewGuid(),
            IsCurrent: false);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
