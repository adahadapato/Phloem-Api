namespace Phloem.Domain.Entities;

/// <summary>
/// One saved wording of an AI prompt (Admin → AI). Never changed: saving, restoring or resetting adds a new version,
/// so every earlier wording can be brought back and the AI log can say which wording produced an answer.
/// </summary>
/// <remarks>The highest <see cref="Version"/> for a <see cref="Key"/> is the one in use. No versions = Phloem's
/// default wording (version 0).</remarks>
public class AiPromptVersion : Entity<Guid>
{
    /// <summary>Longest prompt wording.</summary>
    public const int TextMaxLength = 8000;

    /// <summary>Longest note.</summary>
    public const int NoteMaxLength = 200;

    /// <summary>Longest key.</summary>
    public const int KeyMaxLength = 64;

    /// <summary>For EF Core.</summary>
    protected AiPromptVersion() { }

    private AiPromptVersion(Guid id) : base(id) { }

    /// <summary>The prompt's key, e.g. "prompt.rewrite".</summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>1, 2, 3… per key.</summary>
    public int Version { get; private set; }

    /// <summary>The wording.</summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>Why it changed, e.g. "Shorter titles" or "Restored version 2".</summary>
    public string? Note { get; private set; }

    /// <summary>Who saved it.</summary>
    public Guid SavedBy { get; private set; }

    /// <summary>Makes a new version (not yet saved).</summary>
    /// <param name="key">The prompt's key.</param>
    /// <param name="version">The next number for this key (1 for the first).</param>
    /// <param name="text">The wording (trimmed).</param>
    /// <param name="note">Optional note (cut to <see cref="NoteMaxLength"/>).</param>
    /// <param name="savedBy">Who saved it.</param>
    /// <returns>The new version.</returns>
    /// <exception cref="ArgumentException">Missing key, text or person; text too long; version below 1.</exception>
    public static AiPromptVersion Create(string key, int version, string text, string? note, Guid savedBy)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > KeyMaxLength)
            throw new ArgumentException("A prompt key is required.", nameof(key));
        if (version < 1) throw new ArgumentException("Versions start at 1.", nameof(version));
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("The wording is required.", nameof(text));
        if (text.Trim().Length > TextMaxLength)
            throw new ArgumentException($"The wording can be at most {TextMaxLength} characters.", nameof(text));
        if (savedBy == Guid.Empty) throw new ArgumentException("Who saved it is required.", nameof(savedBy));

        var cleanNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (cleanNote is { Length: > NoteMaxLength }) cleanNote = cleanNote[..NoteMaxLength];

        return new AiPromptVersion(Guid.CreateVersion7())
        {
            Key = key,
            Version = version,
            Text = text.Trim(),
            Note = cleanNote,
            SavedBy = savedBy
        };
    }
}
