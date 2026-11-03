using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Ai.Dtos;

/// <summary>The bar at the top of Admin → AI: on or off, which model, and this month's spend.</summary>
/// <param name="Provider">The provider in the settings.</param>
/// <param name="Model">The default model.</param>
/// <param name="IsEnabled">AI calls can be made.</param>
/// <param name="NotReadyReason">Why it's off, e.g. "No Anthropic API key yet"; null when on.</param>
/// <param name="MonthCalls">AI calls this calendar month.</param>
/// <param name="MonthFailedCalls">Of those, how many failed (service down or unusable answer).</param>
/// <param name="MonthInputTokens">Tokens sent this month.</param>
/// <param name="MonthOutputTokens">Tokens received this month.</param>
/// <param name="MonthCostUsd">This month's spend (US dollars).</param>
/// <param name="MonthlyCapUsd">The most allowed per month.</param>
/// <param name="CapReached">The cap is reached: AI jobs use their fallbacks until next month.</param>
/// <param name="LogRetentionMonths">Months the AI call log is kept.</param>
public sealed record AiStatusDto(
    AiProvider Provider,
    string Model,
    bool IsEnabled,
    string? NotReadyReason,
    int MonthCalls,
    int MonthFailedCalls,
    long MonthInputTokens,
    long MonthOutputTokens,
    decimal MonthCostUsd,
    decimal MonthlyCapUsd,
    bool CapReached,
    int LogRetentionMonths);
