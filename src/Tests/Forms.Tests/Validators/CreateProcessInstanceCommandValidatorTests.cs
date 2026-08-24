using DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.CreateProcessInstance;
using DreamTeam.Modules.Forms.Features.v1.ProcessInstances.CreateProcessInstance;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class CreateProcessInstanceCommandValidatorTests
{
    private readonly CreateProcessInstanceCommandValidator _sut = new();

    private static DateTime FutureUtc(int minutesAhead = 60) =>
        DateTime.UtcNow.AddMinutes(minutesAhead);

    [Fact]
    public void Validate_Should_Pass_ForValidCommand()
    {
        // Arrange
        var command = new CreateProcessInstanceCommand(
            FormVersionId: Guid.NewGuid(),
            ScheduledAt: FutureUtc(60),
            PairUserId: "alice");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_ForWholeTeamRitual_WithoutPairUser()
    {
        // Arrange
        var command = new CreateProcessInstanceCommand(
            FormVersionId: Guid.NewGuid(),
            ScheduledAt: FutureUtc(60),
            PairUserId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_FormVersionIdIsEmpty()
    {
        // Arrange
        var command = new CreateProcessInstanceCommand(
            FormVersionId: Guid.Empty,
            ScheduledAt: FutureUtc(60),
            PairUserId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProcessInstanceCommand.FormVersionId));
    }

    [Fact]
    public void Validate_Should_Fail_When_ScheduledAtIsInThePast()
    {
        // Arrange — 5 minutes in the past
        var command = new CreateProcessInstanceCommand(
            FormVersionId: Guid.NewGuid(),
            ScheduledAt: DateTime.UtcNow.AddMinutes(-5),
            PairUserId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProcessInstanceCommand.ScheduledAt));
    }

    [Fact]
    public void Validate_Should_Fail_When_ScheduledAtIsTooFarInTheFuture()
    {
        // Arrange — 6 years ahead
        var command = new CreateProcessInstanceCommand(
            FormVersionId: Guid.NewGuid(),
            ScheduledAt: DateTime.UtcNow.AddYears(6),
            PairUserId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProcessInstanceCommand.ScheduledAt));
    }

    [Fact]
    public void Validate_Should_AllowClockSkewTolerance_ForNearFuture()
    {
        // Arrange — 10 seconds in the future, within the 30s tolerance
        var command = new CreateProcessInstanceCommand(
            FormVersionId: Guid.NewGuid(),
            ScheduledAt: DateTime.UtcNow.AddSeconds(10),
            PairUserId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_PairUserIdExceedsMaxLength()
    {
        // Arrange
        var command = new CreateProcessInstanceCommand(
            FormVersionId: Guid.NewGuid(),
            ScheduledAt: FutureUtc(60),
            PairUserId: new string('a', 65));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProcessInstanceCommand.PairUserId));
    }
}
