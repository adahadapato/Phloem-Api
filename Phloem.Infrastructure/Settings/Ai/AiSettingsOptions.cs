using System.ComponentModel.DataAnnotations;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;

namespace Phloem.Infrastructure.Settings.Ai;

/// <summary>The "Ai" settings (see <see cref="AppSettings.Ai"/>).</summary>
/// <remarks>
/// <para>Missing values use the defaults below. Values outside the ranges stop the API at start-up.</para>
/// <para>A provider that lacks what it needs (Anthropic without a key, OpenAiCompatible without an endpoint) doesn't
/// stop the API: AI simply stays off and Admin → AI says why.</para>
/// </remarks>
public sealed class AiSettingsOptions
{
    /// <summary>The section name.</summary>
    public const string SectionName = AppSettings.Ai.Section;

    /// <summary>Which provider. Default Off.</summary>
    public AiProvider Provider { get; set; } = AiProvider.Off;

    /// <summary>The default model.</summary>
    [Required, MaxLength(100)]
    public string Model { get; set; } = "claude-haiku-5-5";

    /// <summary>The provider's address (OpenAiCompatible: required; Anthropic: blank = Anthropic's own).</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>The API key (secret; never logged or shown).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Most to spend in a calendar month (US dollars).</summary>
    [Range(typeof(decimal), "0", "10000")]
    public decimal MonthlyCapUsd { get; set; } = 20m;

    /// <summary>Price of a million input tokens (US dollars).</summary>
    [Range(typeof(decimal), "0", "1000")]
    public decimal InputUsdPerMillionTokens { get; set; } = 0.10m;

    /// <summary>Price of a million output tokens (US dollars).</summary>
    [Range(typeof(decimal), "0", "1000")]
    public decimal OutputUsdPerMillionTokens { get; set; } = 0.50m;

    /// <summary>Seconds to wait for one answer.</summary>
    [Range(5, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Months the AI call log is kept.</summary>
    [Range(1, 60)]
    public int LogRetentionMonths { get; set; } = 12;

    /// <summary>Optional model per job (prompt key → model name).</summary>
    public Dictionary<string, string> JobModels { get; set; } = [];

    /// <summary>Why AI can't run with these settings, or null when it can.</summary>
    public string? NotReadyReason() => Provider switch
    {
        AiProvider.Off => "AI is switched off in the settings (Ai:Provider).",
        AiProvider.Anthropic when string.IsNullOrWhiteSpace(ApiKey) =>
            "No Anthropic API key yet (GitHub secret AI_API_KEY).",
        AiProvider.OpenAiCompatible when !Uri.TryCreate(Endpoint, UriKind.Absolute, out _) =>
            "No address for the model yet (Ai:Endpoint).",
        _ => null
    };

    /// <summary>The model for a job: its own when set, else the default.</summary>
    /// <param name="promptKey">The job's prompt key.</param>
    public string ModelFor(string promptKey) =>
        JobModels.TryGetValue(promptKey, out var model) && !string.IsNullOrWhiteSpace(model) ? model.Trim() : Model;

    /// <summary>The cost of a call.</summary>
    /// <param name="inputTokens">Tokens sent.</param>
    /// <param name="outputTokens">Tokens received.</param>
    public decimal CostOf(long inputTokens, long outputTokens) =>
        inputTokens / 1_000_000m * InputUsdPerMillionTokens + outputTokens / 1_000_000m * OutputUsdPerMillionTokens;
}
