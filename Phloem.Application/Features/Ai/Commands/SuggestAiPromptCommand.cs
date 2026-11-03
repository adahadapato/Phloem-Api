using MediatR;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Commands;

/// <summary>Admin → AI → "Ask AI to rewrite": a suggested wording. NOTHING is saved; the person reviews it, then
/// saves it as usual if they like it.</summary>
/// <param name="Key">The prompt being edited.</param>
/// <param name="CurrentText">The wording in the editor now (may have unsaved changes).</param>
/// <param name="Request">What to change, e.g. "Make shop titles shorter"; blank = just make it clearer.</param>
public sealed record SuggestAiPromptCommand(string Key, string CurrentText, string? Request)
    : IRequest<Result<AiPromptSuggestionDto>>;
