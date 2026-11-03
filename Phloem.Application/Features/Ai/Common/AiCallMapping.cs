using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Ai.Common;

/// <summary>AI log rows as DTOs.</summary>
internal static class AiCallMapping
{
    /// <summary>The row without its text.</summary>
    /// <param name="c">The log row.</param>
    public static AiCallSummaryDto ToSummary(this AiCallLog c) => new(
        c.Id, c.CreatedAt, c.PromptKey, AiPrompts.Find(c.PromptKey)?.Name ?? c.PromptKey, c.PromptVersion, c.Subject,
        c.Model, c.Outcome, c.InputTokens, c.OutputTokens, c.CostUsd, c.DurationMs, c.FinalDecision);
}
