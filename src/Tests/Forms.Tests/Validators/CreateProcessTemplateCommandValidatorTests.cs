using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.CreateProcessTemplate;
using DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.CreateProcessTemplate;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class CreateProcessTemplateCommandValidatorTests
{
    private readonly CreateProcessTemplateCommandValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForValidCommand()
    {
        // Arrange
        var command = new CreateProcessTemplateCommand(
            Name: "Weekly 1-1",
            Slug: "weekly-1-1",
            Description: "A weekly 1-1 between lead and member",
            Category: "ONE_ON_ONE");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_NameIsEmpty()
    {
        // Arrange
        var command = new CreateProcessTemplateCommand(Name: "", Slug: "x", null, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_Should_Fail_When_NameExceedsMaxLength()
    {
        // Arrange
        var command = new CreateProcessTemplateCommand(
            Name: new string('a', 201),  // 201 chars
            Slug: "x",
            null, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_Should_Fail_When_SlugIsNotKebabCase()
    {
        // Arrange
        var command = new CreateProcessTemplateCommand(
            Name: "Test",
            Slug: "Not_Kebab_Case",
            null, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Slug");
    }

    [Fact]
    public void Validate_Should_Fail_When_SlugHasLeadingDash()
    {
        // Arrange
        var command = new CreateProcessTemplateCommand(
            Name: "Test",
            Slug: "-leading-dash",
            null, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Slug");
    }

    [Fact]
    public void Validate_Should_AcceptCanonicalSlugShapes()
    {
        // The slug regex: ^[a-z0-9]+(-[a-z0-9]+)*$
        var validSlugs = new[] { "a", "abc", "1-1", "weekly-1-1", "skill-wheel", "okr-check-in", "q1-retro" };
        foreach (var slug in validSlugs)
        {
            var command = new CreateProcessTemplateCommand(Name: "Test", Slug: slug, null, null);
            var result = _sut.Validate(command);
            result.IsValid.ShouldBeTrue($"expected '{slug}' to be a valid slug");
        }
    }

    [Fact]
    public void Validate_Should_RejectSlugsWithDoubleDashes()
    {
        var command = new CreateProcessTemplateCommand(
            Name: "Test",
            Slug: "double--dash",
            null, null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Slug");
    }

    [Fact]
    public void Validate_Should_Fail_When_DescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new CreateProcessTemplateCommand(
            Name: "Test",
            Slug: "test",
            Description: new string('a', 2001),  // 2001 chars
            null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_Should_Allow_NullDescriptionAndCategory()
    {
        // Arrange
        var command = new CreateProcessTemplateCommand(
            Name: "Test",
            Slug: "test",
            Description: null,
            Category: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
