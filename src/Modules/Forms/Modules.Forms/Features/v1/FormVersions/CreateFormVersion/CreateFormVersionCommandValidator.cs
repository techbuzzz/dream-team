using System.Text.Json;
using DreamTeam.Modules.Forms.Contracts.v1.FormVersions.CreateFormVersion;
using FluentValidation;

namespace DreamTeam.Modules.Forms.Features.v1.FormVersions.CreateFormVersion;

public sealed class CreateFormVersionCommandValidator : AbstractValidator<CreateFormVersionCommand>
{
    /// <summary>
    /// MVP-1 cap on the form DSL JSON size. A typical 1-1 schema is ~2-5 KB;
    /// 256 KB leaves plenty of headroom for large Skill Wheel matrices and
    /// multi-page forms while keeping a single request from blowing up the
    /// JSONB column. The Postgres jsonb column itself has no practical limit,
    /// but rejecting at the boundary keeps p99 latency predictable.
    /// </summary>
    private const int MaxSchemaLength = 256 * 1024;

    public CreateFormVersionCommandValidator()
    {
        RuleFor(x => x.ProcessTemplateId)
            .NotEmpty().WithMessage("ProcessTemplateId is required.");

        RuleFor(x => x.Schema)
            .NotEmpty().WithMessage("Schema is required.")
            .MaximumLength(MaxSchemaLength)
                .WithMessage($"Schema must not exceed {MaxSchemaLength} characters.")
            // Reject malformed JSON at the boundary — Postgres jsonb would
            // reject it too, but a 400 here is clearer than a 500 from
            // 22P02 (invalid_text_representation).
            .Must(BeValidJson)
                .WithMessage("Schema must be a valid JSON document.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }

    private static bool BeValidJson(string schema)
    {
        if (string.IsNullOrWhiteSpace(schema))
        {
            return false;
        }

        try
        {
            using var _ = JsonDocument.Parse(schema);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
