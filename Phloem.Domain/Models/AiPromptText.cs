namespace Phloem.Domain.Models;

/// <summary>The wording an AI job uses right now.</summary>
/// <param name="Key">The prompt's key.</param>
/// <param name="Text">The wording.</param>
/// <param name="Version">The saved version; 0 = Phloem's default.</param>
public sealed record AiPromptText(string Key, string Text, int Version);
