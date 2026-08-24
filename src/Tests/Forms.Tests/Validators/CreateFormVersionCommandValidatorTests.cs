using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.CreateFormVersion;
using DreamTeam.Modules.Forms.Features.v1.FormVersions.CreateFormVersion;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class CreateFormVersionCommandValidatorTests
{
    private readonly CreateFormVersionCommandValidator _sut = new();

    private const string ValidOneOnOneSchema = """
        {
          "id": "form_v_1a2b",
          "title": "Weekly 1-1",
          "version": 1,
          "pages": [
            {
              "id": "p1",
              "title": "Check-in",
              "elements": [
                { "id": "energy", "type": "rating", "label": "Energy this week", "scale": 5 },
                { "id": "mood",   "type": "longtext", "label": "How are you, really?" }
              ]
            }
          ]
        }
        """;

    [Fact]
    public void Validate_Should_Pass_ForValidCommand()
    {
        // Arrange
        var command = new CreateFormVersionCommand(
            ProcessTemplateId: Guid.NewGuid(),
            Schema: ValidOneOnOneSchema,
            Description: "First version of the weekly 1-1 form");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_ProcessTemplateIdIsEmpty()
    {
        // Arrange
        var command = new CreateFormVersionCommand(
            ProcessTemplateId: Guid.Empty,
            Schema: ValidOneOnOneSchema,
            Description: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateFormVersionCommand.ProcessTemplateId));
    }

    [Fact]
    public void Validate_Should_Fail_When_SchemaIsEmpty()
    {
        // Arrange
        var command = new CreateFormVersionCommand(
            ProcessTemplateId: Guid.NewGuid(),
            Schema: "",
            Description: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateFormVersionCommand.Schema));
    }

    [Fact]
    public void Validate_Should_Fail_When_SchemaIsWhitespace()
    {
        // Arrange
        var command = new CreateFormVersionCommand(
            ProcessTemplateId: Guid.NewGuid(),
            Schema: "   \n\t  ",
            Description: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_SchemaIsNotValidJson()
    {
        // Arrange — missing closing brace, dangling comma
        var command = new CreateFormVersionCommand(
            ProcessTemplateId: Guid.NewGuid(),
            Schema: "{ \"id\": \"form_v_1a2b\", \"title\":, \"x\" }",
            Description: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateFormVersionCommand.Schema));
    }

    [Fact]
    public void Validate_Should_Fail_When_SchemaExceedsMaxLength()
    {
        // Arrange — 256 KB + 1 of valid JSON-looking whitespace
        // We use a string that JsonDocument would actually accept, otherwise
        // the length check is shadowed by the JSON check.
        var oversized = "\"" + new string('a', 256 * 1024) + "\"";

        var command = new CreateFormVersionCommand(
            ProcessTemplateId: Guid.NewGuid(),
            Schema: oversized,
            Description: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateFormVersionCommand.Schema));
    }

    [Fact]
    public void Validate_Should_Fail_When_DescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new CreateFormVersionCommand(
            ProcessTemplateId: Guid.NewGuid(),
            Schema: ValidOneOnOneSchema,
            Description: new string('a', 2001));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateFormVersionCommand.Description));
    }

    [Fact]
    public void Validate_Should_Allow_NullDescription()
    {
        // Arrange
        var command = new CreateFormVersionCommand(
            ProcessTemplateId: Guid.NewGuid(),
            Schema: ValidOneOnOneSchema,
            Description: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
