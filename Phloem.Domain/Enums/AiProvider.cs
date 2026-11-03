namespace Phloem.Domain.Enums;

/// <summary>Which AI model service Phloem talks to (setting <c>Ai:Provider</c>).</summary>
public enum AiProvider
{
    /// <summary>No AI: every job uses its safe fallback.</summary>
    Off = 0,

    /// <summary>Fixed sample answers (tests, and trying the admin pages without a key). Costs nothing.</summary>
    Fake = 1,

    /// <summary>Claude, through Anthropic's API.</summary>
    Anthropic = 2,

    /// <summary>Any model behind an OpenAI-style endpoint (e.g. your own model on Ollama or vLLM).</summary>
    OpenAiCompatible = 3
}
