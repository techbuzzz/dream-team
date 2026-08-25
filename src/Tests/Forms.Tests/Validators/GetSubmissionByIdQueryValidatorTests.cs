using DreamTeam.Modules.Forms.Contracts.v1.Submissions.GetSubmissionById;
using DreamTeam.Modules.Forms.Features.v1.Submissions.GetSubmissionById;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class GetSubmissionByIdQueryValidatorTests
{
    private readonly GetSubmissionByIdQueryValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForNonEmptyId()
    {
        // Arrange
        var query = new GetSubmissionByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdIsEmpty()
    {
        // Arrange
        var query = new GetSubmissionByIdQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetSubmissionByIdQuery.Id));
    }
}
