using System.Text.Json;

namespace Phloem.Domain.Models;

/// <summary>A checked AI answer: JSON with every required field.</summary>
/// <param name="CallId">The call in the AI log (record staff's final decision against it).</param>
/// <param name="Json">The answer's JSON object.</param>
public sealed record AiJsonAnswer(Guid CallId, JsonElement Json)
{
    /// <summary>A text field of the answer, or null when it's missing or not text.</summary>
    /// <param name="name">The field's name.</param>
    public string? Text(string name) =>
        Json.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}
