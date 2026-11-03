namespace Phloem.Domain.Enums;

/// <summary>How one AI call ended.</summary>
public enum AiCallOutcome
{
    /// <summary>A JSON answer with every required field.</summary>
    Succeeded = 1,

    /// <summary>The model service failed (down, timed out, refused the key, rate limit).</summary>
    ProviderFailed = 2,

    /// <summary>The model answered, but not with usable JSON (or a required field was missing).</summary>
    BadAnswer = 3
}
