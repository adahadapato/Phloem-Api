using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Ai;

/// <summary>
/// A pretend model (setting <c>Ai:Provider</c> = <c>Fake</c>): answers every job with its sample answer from
/// <see cref="AiPrompts"/>, at no cost. For tests and for trying Admin → AI before there's a key.
/// </summary>
internal sealed class FakeChatClient : IChatClient
{
    /// <summary>Where the AI client puts the job's key, so the fake knows which sample to return.</summary>
    public const string PromptKeyProperty = "phloem.promptKey";

    /// <inheritdoc />
    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
                                               CancellationToken cancellationToken = default)
    {
        var key = options?.AdditionalProperties?.TryGetValue(PromptKeyProperty, out var value) == true
            ? value as string
            : null;
        var answer = (key is null ? null : AiPrompts.Find(key)?.SampleAnswer) ?? "{}";
        var sent = messages.Sum(m => m.Text?.Length ?? 0);

        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, answer))
        {
            ModelId = "fake",
            Usage = new UsageDetails { InputTokenCount = sent / 4, OutputTokenCount = answer.Length / 4 }
        });
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetResponseAsync(messages, options, cancellationToken);
        foreach (var update in response.ToChatResponseUpdates()) yield return update;
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    /// <inheritdoc />
    public void Dispose() { }
}
