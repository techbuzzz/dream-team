using DreamTeam.Modules.Forms.Contracts.v1.Submissions.CreateSubmission;
using DreamTeam.Modules.Forms.Features.v1.Submissions.CreateSubmission;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class CreateSubmissionCommandValidatorTests
{
    private readonly CreateSubmissionCommandValidator _sut = new();

    private const string ValidOneOnOneData = """
        {
          "energy": 4,
          "mood": "great",
          "blockers": "",
          "focus": "Ship the new auth flow"
        }
        """;

    [Fact]
    public void Validate_Should_Pass_ForValidCommand()
    {
        // Arrange
        var command = new CreateSubmissionCommand(
            ProcessInstanceId: Guid.NewGuid(),
            FormVersionId: Guid.NewGuid(),
            Data: ValidOneOnOneData);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_CompensatesSubmissionIdIsSet()
    {
        // Arrange — append-only correction
        var command = new CreateSubmissionCommand(
            ProcessInstanceId: Guid.NewGuid(),
            FormVersionId: Guid.NewGuid(),
            Data: ValidOneOnOneData,
            CompensatesSubmissionId: Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_ProcessInstanceIdIsEmpty()
    {
        // Arrange
        var command = new CreateSubmissionCommand(
            ProcessInstanceId: Guid.Empty,
            FormVersionId: Guid.NewGuid(),
            Data: ValidOneOnOneData);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSubmissionCommand.ProcessInstanceId));
    }

    [Fact]
    public void Validate_Should_Fail_When_FormVersionIdIsEmpty()
    {
        // Arrange
        var command = new CreateSubmissionCommand(
            ProcessInstanceId: Guid.NewGuid(),
            FormVersionId: Guid.Empty,
            Data: ValidOneOnOneData);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSubmissionCommand.FormVersionId));
    }

    [Fact]
    public void Validate_Should_Fail_When_DataIsEmpty()
    {
        // Arrange
        var command = new CreateSubmissionCommand(
            ProcessInstanceId: Guid.NewGuid(),
            FormVersionId: Guid.NewGuid(),
            Data: "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSubmissionCommand.Data));
    }

    [Fact]
    public void Validate_Should_Fail_When_DataIsNotValidJson()
    {
        // Arrange — missing closing brace
        var command = new CreateSubmissionCommand(
            ProcessInstanceId: Guid.NewGuid(),
            FormVersionId: Guid.NewGuid(),
            Data: "{ \"energy\": 4, \"mood\":");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSubmissionCommand.Data));
    }

    [Fact]
    public void Validate_Should_Fail_When_DataExceedsMaxLength()
    {
        // Arrange — 256 KB + 1 of valid JSON
        var oversized = "\"" + new string('a', 256 * 1024) + "\"";

        var command = new CreateSubmissionCommand(
            ProcessInstanceId: Guid.NewGuid(),
            FormVersionId: Guid.NewGuid(),
            Data: oversized);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSubmissionCommand.Data));
    }
}
