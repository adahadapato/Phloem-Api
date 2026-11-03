namespace Phloem.Domain.Shared.Errors;

/// <summary>Every AI failure. Nothing here is thrown: each AI job decides its own safe fallback.</summary>
public static class AiErrors
{
    /// <summary>AI is switched off, or not set up yet (no key).</summary>
    public static readonly Error Disabled = Error.Failure(
        "Ai.Disabled", "AI isn't switched on yet.");

    /// <summary>This month's spend cap is reached.</summary>
    public static readonly Error SpendCapReached = Error.Failure(
        "Ai.SpendCapReached", "This month's AI spending limit has been reached. AI starts again next month.");

    /// <summary>The model service failed (down, timed out, wrong key, too many requests).</summary>
    public static readonly Error ProviderFailed = Error.Failure(
        "Ai.ProviderFailed", "The AI service didn't answer. Please try again in a moment.");

    /// <summary>The model answered, but not in the shape the job needs.</summary>
    public static readonly Error BadAnswer = Error.Failure(
        "Ai.BadAnswer", "The AI's answer couldn't be used. Please try again.");

    /// <summary>No such prompt.</summary>
    public static readonly Error PromptNotFound = Error.NotFound(
        "Ai.PromptNotFound", "There's no AI prompt with that name.");

    /// <summary>No such saved version of the prompt.</summary>
    public static readonly Error VersionNotFound = Error.NotFound(
        "Ai.VersionNotFound", "That version of the prompt doesn't exist.");

    /// <summary>No such call in the AI log (or it has been deleted after the keeping period).</summary>
    public static readonly Error CallNotFound = Error.NotFound(
        "Ai.CallNotFound", "That AI call isn't in the log.");

    /// <summary>The wording no longer mentions a field the job's answer needs.</summary>
    /// <param name="missing">The missing fields.</param>
    public static Error MissingFields(IEnumerable<string> missing) => Error.Validation(
        "Ai.MissingFields",
        $"The prompt must still ask for these answer fields: {string.Join(", ", missing)}.");

    /// <summary>Nothing changed: the wording is the same as the one in use.</summary>
    public static readonly Error NoChange = Error.Validation(
        "Ai.NoChange", "That's the wording already in use.");
}
