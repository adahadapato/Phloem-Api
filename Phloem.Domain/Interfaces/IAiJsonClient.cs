using Phloem.Domain.Models;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// Asks the AI model one question and returns a checked JSON answer. Every AI job goes through this.
/// </summary>
/// <remarks>
/// <para>Uses the job's current wording, refuses to call when AI is off or this month's cap is reached, strips code
/// fences, checks the job's required fields, and writes every call to the AI log (its own save, so the caller's
/// changes are never saved by accident).</para>
/// <para>Never throws for AI problems: it returns an <c>AiErrors</c> failure and the caller uses its fallback.</para>
/// </remarks>
public interface IAiJsonClient
{
    /// <summary>Asks the question.</summary>
    /// <param name="request">The job, its input and what it's about.</param>
    /// <param name="cancellationToken">Stops the call.</param>
    /// <returns>The answer; or Disabled / SpendCapReached / ProviderFailed / BadAnswer.</returns>
    Task<Result<AiJsonAnswer>> AskAsync(AiJsonRequest request, CancellationToken cancellationToken = default);
}
