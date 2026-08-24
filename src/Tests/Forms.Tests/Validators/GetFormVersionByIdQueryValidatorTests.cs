using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetFormVersionById;
using DreamTeam.Modules.Forms.Features.v1.FormVersions.GetFormVersionById;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class GetFormVersionByIdQueryValidatorTests
{
    private readonly GetFormVersionByIdQueryValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForNonEmptyId()
    {
        // Arrange
        var query = new GetFormVersionByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdIsEmpty()
    {
        // Arrange
        var query = new GetFormVersionByIdQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetFormVersionByIdQuery.Id));
    }
}
