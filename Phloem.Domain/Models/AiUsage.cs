namespace Phloem.Domain.Models;

/// <summary>AI use over a period.</summary>
/// <param name="Calls">Calls made (including failed ones).</param>
/// <param name="FailedCalls">Calls that failed (service down or unusable answer).</param>
/// <param name="InputTokens">Tokens sent.</param>
/// <param name="OutputTokens">Tokens received.</param>
/// <param name="CostUsd">What it cost (US dollars, from the prices in settings).</param>
public sealed record AiUsage(int Calls, int FailedCalls, long InputTokens, long OutputTokens, decimal CostUsd)
{
    /// <summary>Nothing used.</summary>
    public static AiUsage None { get; } = new(0, 0, 0, 0, 0m);
}
