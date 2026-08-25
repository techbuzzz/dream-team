using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.UpdateProcessInstance;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.UpdateProcessInstance;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class UpdateProcessInstanceCommandValidatorTests
{
    private readonly UpdateProcessInstanceCommandValidator _sut = new();

    private static DateTime FutureUtc(int minutesAhead = 60) =>
        DateTime.UtcNow.AddMinutes(minutesAhead);

    [Fact]
    public void Validate_Should_Pass_When_ScheduledAtSupplied()
    {
        // Arrange
        var command = new UpdateProcessInstanceCommand(
            Id: Guid.NewGuid(),
            ScheduledAt: FutureUtc(60),
            PairUserId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_PairUserIdSupplied()
    {
        // Arrange
        var command = new UpdateProcessInstanceCommand(
            Id: Guid.NewGuid(),
            ScheduledAt: null,
            PairUserId: "alice");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_BothFieldsSupplied()
    {
        // Arrange
        var command = new UpdateProcessInstanceCommand(
            Id: Guid.NewGuid(),
            ScheduledAt: FutureUtc(60),
            PairUserId: "alice");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_AllFieldsAreNull()
    {
        // Arrange
        var command = new UpdateProcessInstanceCommand(
            Id: Guid.NewGuid(),
            ScheduledAt: null,
            PairUserId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("At least one of ScheduledAt or PairUserId"));
    }

    [Fact]
    public void Validate_Should_Fail_When_IdIsEmpty()
    {
        // Arrange
        var command = new UpdateProcessInstanceCommand(
            Id: Guid.Empty,
            ScheduledAt: FutureUtc(60),
            PairUserId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateProcessInstanceCommand.Id));
    }

    [Fact]
    public void Validate_Should_Fail_When_ScheduledAtIsInThePast()
    {
        // Arrange
        var command = new UpdateProcessInstanceCommand(
            Id: Guid.NewGuid(),
            ScheduledAt: DateTime.UtcNow.AddMinutes(-5),
            PairUserId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_PairUserIdIsEmpty()
    {
        // Arrange
        var command = new UpdateProcessInstanceCommand(
            Id: Guid.NewGuid(),
            ScheduledAt: null,
            PairUserId: "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_PairUserIdExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateProcessInstanceCommand(
            Id: Guid.NewGuid(),
            ScheduledAt: null,
            PairUserId: new string('a', 65));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }
}
