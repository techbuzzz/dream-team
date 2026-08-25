using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByTemplateId;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByTemplateId;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class GetProcessInstancesByTemplateIdQueryValidatorTests
{
    private readonly GetProcessInstancesByTemplateIdQueryValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForValidQuery()
    {
        // Arrange
        var query = new GetProcessInstancesByTemplateIdQuery(
            TemplateId: Guid.NewGuid(),
            PageNumber: 1,
            PageSize: 20,
            Sort: "ScheduledAt");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_TemplateIdIsEmpty()
    {
        // Arrange
        var query = new GetProcessInstancesByTemplateIdQuery(TemplateId: Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetProcessInstancesByTemplateIdQuery.TemplateId));
    }

    [Fact]
    public void Validate_Should_Accept_ScheduledAtDesc_Sort()
    {
        // Arrange
        var query = new GetProcessInstancesByTemplateIdQuery(
            TemplateId: Guid.NewGuid(),
            Sort: "ScheduledAtDesc");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Accept_CreatedOnUtcDesc_Sort()
    {
        // Arrange
        var query = new GetProcessInstancesByTemplateIdQuery(
            TemplateId: Guid.NewGuid(),
            Sort: "CreatedOnUtcDesc");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
