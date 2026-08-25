using DreamTeam.Modules.Forms.Domain;
using Shouldly;
using Xunit;

namespace Forms.Tests.Domain;

/// <summary>
/// Domain tests for the Submission entity. Pins the append-only invariants:
/// a freshly-submitted row has <c>IsCompensating</c> derived from whether
/// <c>CompensatesSubmissionId</c> is set, and the row is a fresh INSERT —
/// there's no public API to mutate it after creation.
/// </summary>
public sealed class SubmissionTests
{
    [Fact]
    public void Submit_Should_CreateOriginalRow_WithIsCompensatingFalse()
    {
        // Arrange
        var processInstanceId = Guid.NewGuid();
        var formVersionId = Guid.NewGuid();
        var tenantId = "tenant-1";
        var authorId = "user-42";
        var data = """{"energy": 4, "mood": "great"}""";

        // Act
        var submission = Submission.Submit(
            processInstanceId: processInstanceId,
            formVersionId: formVersionId,
            tenantId: tenantId,
            authorId: authorId,
            data: data);

        // Assert
        submission.Id.ShouldNotBe(Guid.Empty);
        submission.ProcessInstanceId.ShouldBe(processInstanceId);
        submission.FormVersionId.ShouldBe(formVersionId);
        submission.TenantId.ShouldBe(tenantId);
        submission.AuthorId.ShouldBe(authorId);
        submission.Data.ShouldBe(data);
        submission.IsCompensating.ShouldBeFalse();
        submission.CompensatesSubmissionId.ShouldBeNull();
    }

    [Fact]
    public void Submit_Should_CreateCompensatingRow_WhenPriorSubmissionIdSet()
    {
        // Arrange
        var priorId = Guid.NewGuid();
        var submission = Submission.Submit(
            processInstanceId: Guid.NewGuid(),
            formVersionId: Guid.NewGuid(),
            tenantId: "tenant-1",
            authorId: "user-42",
            data: """{"energy": 3}""",
            isCompensating: true,
            compensatesSubmissionId: priorId);

        // Assert
        submission.IsCompensating.ShouldBeTrue();
        submission.CompensatesSubmissionId.ShouldBe(priorId);
    }

    [Fact]
    public void Submit_Should_Throw_When_TenantIdIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            Submission.Submit(
                processInstanceId: Guid.NewGuid(),
                formVersionId: Guid.NewGuid(),
                tenantId: "",
                authorId: "user-1",
                data: "{}"));
    }

    [Fact]
    public void Submit_Should_Throw_When_AuthorIdIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            Submission.Submit(
                processInstanceId: Guid.NewGuid(),
                formVersionId: Guid.NewGuid(),
                tenantId: "tenant-1",
                authorId: "",
                data: "{}"));
    }

    [Fact]
    public void Submit_Should_Throw_When_DataIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            Submission.Submit(
                processInstanceId: Guid.NewGuid(),
                formVersionId: Guid.NewGuid(),
                tenantId: "tenant-1",
                authorId: "user-1",
                data: ""));
    }
}
