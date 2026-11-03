using FluentValidation;
using Phloem.Application.Features.Ai.Commands;
using Phloem.Application.Features.Ai.Queries;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Ai.Validators;

/// <summary>Input checks for <see cref="SaveAiPromptCommand"/>.</summary>
public sealed class SaveAiPromptCommandValidator : AbstractValidator<SaveAiPromptCommand>
{
    /// <summary>Key and wording required; wording and note within their lengths.</summary>
    public SaveAiPromptCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(AiPromptVersion.KeyMaxLength);
        RuleFor(x => x.Text).NotEmpty().WithMessage("Write the prompt's wording.")
                            .MaximumLength(AiPromptVersion.TextMaxLength);
        RuleFor(x => x.Note).MaximumLength(AiPromptVersion.NoteMaxLength);
    }
}

/// <summary>Input checks for <see cref="RestoreAiPromptVersionCommand"/>.</summary>
public sealed class RestoreAiPromptVersionCommandValidator : AbstractValidator<RestoreAiPromptVersionCommand>
{
    /// <summary>Key required; versions start at 1.</summary>
    public RestoreAiPromptVersionCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(AiPromptVersion.KeyMaxLength);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}

/// <summary>Input checks for <see cref="ResetAiPromptCommand"/>.</summary>
public sealed class ResetAiPromptCommandValidator : AbstractValidator<ResetAiPromptCommand>
{
    /// <summary>Key required.</summary>
    public ResetAiPromptCommandValidator() =>
        RuleFor(x => x.Key).NotEmpty().MaximumLength(AiPromptVersion.KeyMaxLength);
}

/// <summary>Input checks for <see cref="SuggestAiPromptCommand"/>.</summary>
public sealed class SuggestAiPromptCommandValidator : AbstractValidator<SuggestAiPromptCommand>
{
    /// <summary>Longest "what to change".</summary>
    public const int RequestMaxLength = 500;

    /// <summary>Key and the current wording required; the request is short.</summary>
    public SuggestAiPromptCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(AiPromptVersion.KeyMaxLength);
        RuleFor(x => x.CurrentText).NotEmpty().MaximumLength(AiPromptVersion.TextMaxLength);
        RuleFor(x => x.Request).MaximumLength(RequestMaxLength);
    }
}

/// <summary>Input checks for <see cref="GetAiPromptQuery"/>.</summary>
public sealed class GetAiPromptQueryValidator : AbstractValidator<GetAiPromptQuery>
{
    /// <summary>Key required.</summary>
    public GetAiPromptQueryValidator() =>
        RuleFor(x => x.Key).NotEmpty().MaximumLength(AiPromptVersion.KeyMaxLength);
}

/// <summary>Input checks for <see cref="GetAiCallsQuery"/>.</summary>
public sealed class GetAiCallsQueryValidator : AbstractValidator<GetAiCallsQuery>
{
    /// <summary>Filters must be real values when given.</summary>
    public GetAiCallsQueryValidator()
    {
        RuleFor(x => x.PromptKey).MaximumLength(AiPromptVersion.KeyMaxLength);
        RuleFor(x => x.Outcome).IsInEnum();
    }
}
