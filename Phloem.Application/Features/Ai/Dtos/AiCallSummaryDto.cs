using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Ai.Dtos;

/// <summary>One row of the AI call log (no text: open the call to see it).</summary>
/// <param name="Id">The call.</param>
/// <param name="At">When (UTC).</param>
/// <param name="PromptKey">The job.</param>
/// <param name="PromptName">The job's name.</param>
/// <param name="PromptVersion">The wording's version (0 = default).</param>
/// <param name="Subject">What it was about.</param>
/// <param name="Model">The model.</param>
/// <param name="Outcome">How it ended.</param>
/// <param name="InputTokens">Tokens sent.</param>
/// <param name="OutputTokens">Tokens received.</param>
/// <param name="CostUsd">Cost (US dollars).</param>
/// <param name="DurationMs">How long it took.</param>
/// <param name="FinalDecision">What staff decided, when recorded.</param>
public sealed record AiCallSummaryDto(
    Guid Id,
    DateTime At,
    string PromptKey,
    string PromptName,
    int PromptVersion,
    string? Subject,
    string Model,
    AiCallOutcome Outcome,
    int InputTokens,
    int OutputTokens,
    decimal CostUsd,
    int DurationMs,
    string? FinalDecision);
