using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using Phloem.Domain.Enums;
using System.ClientModel;

namespace Phloem.Infrastructure.Settings.Ai;

/// <summary>
/// The connection to the AI model, made once from the settings: Claude (Anthropic), any OpenAI-style endpoint (e.g.
/// your own model on Ollama or vLLM), the fake model, or none.
/// </summary>
/// <remarks>Everything else talks to <see cref="IChatClient"/> (Microsoft.Extensions.AI), so changing provider is a
/// settings change, not a code change.</remarks>
internal sealed class AiModel : IDisposable
{
    /// <summary>Makes the connection the settings ask for.</summary>
    /// <param name="options">The AI settings.</param>
    /// <param name="logger">Says which model is in use at start-up (never the key).</param>
    public AiModel(IOptions<AiSettingsOptions> options, ILogger<AiModel> logger)
    {
        var o = options.Value;
        var reason = o.NotReadyReason();
        if (reason is not null)
        {
            logger.LogInformation("AI is off: {Reason}", reason);
            return;
        }

        var timeout = TimeSpan.FromSeconds(o.TimeoutSeconds);
        Client = o.Provider switch
        {
            AiProvider.Fake => new FakeChatClient(),
            AiProvider.Anthropic => CreateAnthropic(o, timeout),
            AiProvider.OpenAiCompatible => CreateOpenAiCompatible(o, timeout),
            _ => null
        };
        logger.LogInformation("AI is on: {Provider}, model {Model}.", o.Provider, o.Model);
    }

    /// <summary>The model; null when AI is off.</summary>
    public IChatClient? Client { get; }

    /// <inheritdoc />
    public void Dispose() => Client?.Dispose();

    private static IChatClient CreateAnthropic(AiSettingsOptions o, TimeSpan timeout)
    {
        var client = string.IsNullOrWhiteSpace(o.Endpoint)
            ? new AnthropicClient { ApiKey = o.ApiKey, Timeout = timeout, MaxRetries = 2 }
            : new AnthropicClient { ApiKey = o.ApiKey, BaseUrl = o.Endpoint.Trim(), Timeout = timeout, MaxRetries = 2 };
        return client.AsIChatClient(o.Model);
    }

    private static IChatClient CreateOpenAiCompatible(AiSettingsOptions o, TimeSpan timeout)
    {
        // Local servers (Ollama) need no key, but the library needs one: any text works there
        var key = string.IsNullOrWhiteSpace(o.ApiKey) ? "no-key" : o.ApiKey;
        var settings = new OpenAIClientOptions { Endpoint = new Uri(o.Endpoint.Trim()), NetworkTimeout = timeout };
        return new OpenAI.Chat.ChatClient(o.Model, new ApiKeyCredential(key), settings).AsIChatClient();
    }
}
