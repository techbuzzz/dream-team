using DreamTeam.Modules.Forms.Contracts.v1.ProcessTemplates.GetProcessTemplates;
using DreamTeam.Modules.Forms.Features.v1.ProcessTemplates.GetProcessTemplates;
using Shouldly;
using Xunit;

namespace Forms.Tests.Validators;

public sealed class GetProcessTemplatesQueryValidatorTests
{
    private readonly GetProcessTemplatesQueryValidator _sut = new();

    [Fact]
    public void Validate_Should_Pass_ForDefaults()
    {
        // Default page=1, size=20, no search — the most common read path.
        var result = _sut.Validate(new GetProcessTemplatesQuery());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_WhenPageSizeExceedsMax()
    {
        // PagedQueryValidator caps at 100.
        var result = _sut.Validate(new GetProcessTemplatesQuery(PageSize: 500));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Validate_Should_Fail_WhenPageSizeIsZero()
    {
        var result = _sut.Validate(new GetProcessTemplatesQuery(PageSize: 0));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Validate_Should_Fail_WhenPageNumberIsZero()
    {
        var result = _sut.Validate(new GetProcessTemplatesQuery(PageNumber: 0));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PageNumber");
    }

    [Fact]
    public void Validate_Should_Fail_WhenSearchTermExceedsMaxLength()
    {
        var result = _sut.Validate(new GetProcessTemplatesQuery(SearchTerm: new string('a', 201)));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "SearchTerm");
    }

    [Fact]
    public void Validate_Should_Pass_WhenSearchTermIsAtMaxLength()
    {
        var result = _sut.Validate(new GetProcessTemplatesQuery(SearchTerm: new string('a', 200)));
        result.IsValid.ShouldBeTrue();
    }
}
