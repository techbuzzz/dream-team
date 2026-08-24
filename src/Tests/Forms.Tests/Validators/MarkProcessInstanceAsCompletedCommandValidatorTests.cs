using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.MarkProcessInstanceAsCompleted;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.MarkProcessInstanceAsCompleted;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class MarkProcessInstanceAsCompletedCommandValidatorTests
{
    private readonly MarkProcessInstanceAsCompletedCommandValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForNonEmptyInstanceId()
    {
        // Arrange
        var command = new MarkProcessInstanceAsCompletedCommand(Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_InstanceIdIsEmpty()
    {
        // Arrange
        var command = new MarkProcessInstanceAsCompletedCommand(Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(MarkProcessInstanceAsCompletedCommand.InstanceId));
    }
}
