using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstanceById;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.GetProcessInstanceById;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class GetProcessInstanceByIdQueryValidatorTests
{
    private readonly GetProcessInstanceByIdQueryValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForNonEmptyId()
    {
        // Arrange
        var query = new GetProcessInstanceByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdIsEmpty()
    {
        // Arrange
        var query = new GetProcessInstanceByIdQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetProcessInstanceByIdQuery.Id));
    }
}
