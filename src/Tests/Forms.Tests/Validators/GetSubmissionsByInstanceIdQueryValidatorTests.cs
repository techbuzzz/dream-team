using DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionsByInstanceId;
using DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionsByInstanceId;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class GetSubmissionsByInstanceIdQueryValidatorTests
{
    private readonly GetSubmissionsByInstanceIdQueryValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForValidQuery()
    {
        // Arrange
        var query = new GetSubmissionsByInstanceIdQuery(
            InstanceId: Guid.NewGuid(),
            PageNumber: 1,
            PageSize: 20,
            Sort: "CreatedOnUtc");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_InstanceIdIsEmpty()
    {
        // Arrange
        var query = new GetSubmissionsByInstanceIdQuery(InstanceId: Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetSubmissionsByInstanceIdQuery.InstanceId));
    }

    [Fact]
    public void Validate_Should_Accept_CreatedOnUtcDesc_Sort()
    {
        // Arrange
        var query = new GetSubmissionsByInstanceIdQuery(
            InstanceId: Guid.NewGuid(),
            Sort: "CreatedOnUtcDesc");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Accept_NullSort_AsDefault()
    {
        // Arrange
        var query = new GetSubmissionsByInstanceIdQuery(
            InstanceId: Guid.NewGuid(),
            Sort: null);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
