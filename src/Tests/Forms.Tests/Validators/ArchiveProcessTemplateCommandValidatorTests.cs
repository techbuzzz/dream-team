using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.ArchiveProcessTemplate;
using DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.ArchiveProcessTemplate;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class ArchiveProcessTemplateCommandValidatorTests
{
    private readonly ArchiveProcessTemplateCommandValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForNonEmptyId()
    {
        // Arrange
        var command = new ArchiveProcessTemplateCommand(Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdIsEmpty()
    {
        // Arrange
        var command = new ArchiveProcessTemplateCommand(Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ArchiveProcessTemplateCommand.Id));
    }
}
