using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.GetCurrentFormVersion;
using DreamTeam.Modules.Forms.Features.v1.FormVersions.GetCurrentFormVersion;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class GetCurrentFormVersionQueryValidatorTests
{
    private readonly GetCurrentFormVersionQueryValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForNonEmptyTemplateId()
    {
        // Arrange
        var query = new GetCurrentFormVersionQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_TemplateIdIsEmpty()
    {
        // Arrange
        var query = new GetCurrentFormVersionQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetCurrentFormVersionQuery.TemplateId));
    }
}
