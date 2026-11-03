using System.Text.Json;

namespace Phloem.Infrastructure.Settings.Ai;

/// <summary>Turns a model's reply into one JSON object (models sometimes wrap it in ```json fences or a sentence).</summary>
internal static class AiAnswerParser
{
    /// <summary>The JSON object in the reply, or null when there isn't a readable one.</summary>
    /// <param name="reply">The model's text.</param>
    public static JsonElement? ReadObject(string? reply)
    {
        if (string.IsNullOrWhiteSpace(reply)) return null;

        // From the first "{" to the last "}": drops fences and any words around the object
        var start = reply.IndexOf('{');
        var end = reply.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        try
        {
            using var document = JsonDocument.Parse(reply.AsMemory(start, end - start + 1));
            return document.RootElement.ValueKind == JsonValueKind.Object ? document.RootElement.Clone() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>The required fields the answer lacks (a field set to null counts as missing).</summary>
    /// <param name="answer">The JSON object.</param>
    /// <param name="required">The fields the job needs.</param>
    public static IReadOnlyList<string> MissingFields(JsonElement answer, IEnumerable<string> required) =>
        required.Where(f => !answer.TryGetProperty(f, out var v) || v.ValueKind == JsonValueKind.Null).ToList();
}
