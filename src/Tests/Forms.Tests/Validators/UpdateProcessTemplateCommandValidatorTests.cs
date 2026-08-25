using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.UpdateProcessTemplate;
using DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.UpdateProcessTemplate;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class UpdateProcessTemplateCommandValidatorTests
{
    private readonly UpdateProcessTemplateCommandValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_When_NameSupplied()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.NewGuid(),
            Name: "New Name",
            Description: null,
            Category: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_DescriptionSupplied()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.NewGuid(),
            Name: null,
            Description: "Updated description",
            Category: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_CategorySupplied()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.NewGuid(),
            Name: null,
            Description: null,
            Category: "RETRO");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_AllFieldsSupplied()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.NewGuid(),
            Name: "All",
            Description: "All",
            Category: "ALL");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_AllFieldsAreNull()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.NewGuid(),
            Name: null,
            Description: null,
            Category: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("At least one of Name, Description, or Category"));
    }

    [Fact]
    public void Validate_Should_Fail_When_IdIsEmpty()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.Empty,
            Name: "Test",
            Description: null,
            Category: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateProcessTemplateCommand.Id));
    }

    [Fact]
    public void Validate_Should_Fail_When_NameIsEmptyString()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.NewGuid(),
            Name: "",
            Description: null,
            Category: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_NameExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.NewGuid(),
            Name: new string('a', 201),
            Description: null,
            Category: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_DescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.NewGuid(),
            Name: null,
            Description: new string('a', 2001),
            Category: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_CategoryExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateProcessTemplateCommand(
            Id: Guid.NewGuid(),
            Name: null,
            Description: null,
            Category: new string('a', 65));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }
}
