using Phloem.Domain.Enums;

namespace Phloem.Domain.Models;

/// <summary>The AI settings that matter outside Infrastructure (no key).</summary>
/// <param name="Provider">The provider chosen in settings.</param>
/// <param name="Model">The default model.</param>
/// <param name="IsEnabled">True when AI calls can be made (a provider that has what it needs, e.g. a key).</param>
/// <param name="NotReadyReason">Why AI is off, e.g. "No API key yet"; null when enabled.</param>
/// <param name="MonthlyCapUsd">Most to spend in a calendar month.</param>
/// <param name="LogRetentionMonths">Months the AI call log is kept.</param>
public sealed record AiSettings(
    AiProvider Provider,
    string Model,
    bool IsEnabled,
    string? NotReadyReason,
    decimal MonthlyCapUsd,
    int LogRetentionMonths);
