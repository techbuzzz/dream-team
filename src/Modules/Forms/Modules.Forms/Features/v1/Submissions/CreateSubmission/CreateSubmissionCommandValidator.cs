using System.Text.Json;
using DreamTeam.Modules.Forms.Contracts.v1.Submissions.CreateSubmission;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.Submissions.CreateSubmission;

public sealed class CreateSubmissionCommandValidator : AbstractValidator<CreateSubmissionCommand>
{
    /// <summary>
    /// Same cap as FormVersion schema. A typical 1-1 submission is 1-2 KB;
    /// 256 KB leaves headroom for the largest expected forms (Skill Wheel
    /// matrices, multi-page retros with rich text).
    /// </summary>
    private const int MaxDataLength = 256 * 1024;

    public CreateSubmissionCommandValidator()
    {
        RuleFor(x => x.ProcessInstanceId)
            .NotEmpty().WithMessage("ProcessInstanceId is required.");

        RuleFor(x => x.FormVersionId)
            .NotEmpty().WithMessage("FormVersionId is required.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("Data is required.")
            .MaximumLength(MaxDataLength)
                .WithMessage($"Data must not exceed {MaxDataLength} characters.")
            .Must(BeValidJson)
                .WithMessage("Data must be a valid JSON document.");
    }

    private static bool BeValidJson(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return false;
        }

        try
        {
            using var _ = JsonDocument.Parse(data);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
