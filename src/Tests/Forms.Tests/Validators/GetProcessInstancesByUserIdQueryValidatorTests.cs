using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstancesByUserId;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstancesByUserId;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class GetProcessInstancesByUserIdQueryValidatorTests
{
    private readonly GetProcessInstancesByUserIdQueryValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForValidQuery()
    {
        // Arrange
        var query = new GetProcessInstancesByUserIdQuery(
            UserId: "alice",
            PageNumber: 1,
            PageSize: 20,
            Sort: "ScheduledAt");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_UserIdIsEmpty()
    {
        // Arrange
        var query = new GetProcessInstancesByUserIdQuery(UserId: "");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetProcessInstancesByUserIdQuery.UserId));
    }

    [Fact]
    public void Validate_Should_Fail_When_UserIdExceedsMaxLength()
    {
        // Arrange
        var query = new GetProcessInstancesByUserIdQuery(UserId: new string('a', 65));

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetProcessInstancesByUserIdQuery.UserId));
    }

    [Fact]
    public void Validate_Should_Accept_ScheduledAtDesc_Sort()
    {
        // Arrange
        var query = new GetProcessInstancesByUserIdQuery(
            UserId: "alice",
            Sort: "ScheduledAtDesc");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
