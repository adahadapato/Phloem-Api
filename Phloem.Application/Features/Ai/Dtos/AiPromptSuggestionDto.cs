namespace Phloem.Application.Features.Ai.Dtos;

/// <summary>"Ask AI to rewrite": a suggested wording (not saved).</summary>
/// <param name="Text">The suggested wording.</param>
/// <param name="Summary">What the AI changed and why.</param>
/// <param name="MissingFields">Required answer fields the suggestion forgot (it can't be saved until they're
/// added back); empty when it's fine.</param>
public sealed record AiPromptSuggestionDto(string Text, string Summary, IReadOnlyList<string> MissingFields);
