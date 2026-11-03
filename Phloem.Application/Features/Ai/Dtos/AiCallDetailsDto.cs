namespace Phloem.Application.Features.Ai.Dtos;

/// <summary>One AI call with what went in and what came back (opening it is audited).</summary>
/// <param name="Call">The row.</param>
/// <param name="Input">What was sent (after the instructions).</param>
/// <param name="Output">What came back; null if nothing.</param>
/// <param name="Error">Why it failed.</param>
/// <param name="DecidedBy">Who recorded the final decision.</param>
/// <param name="DecidedAt">When.</param>
public sealed record AiCallDetailsDto(
    AiCallSummaryDto Call,
    string Input,
    string? Output,
    string? Error,
    string? DecidedBy,
    DateTime? DecidedAt);
