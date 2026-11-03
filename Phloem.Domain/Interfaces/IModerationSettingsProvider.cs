namespace Phloem.Domain.Interfaces;

/// <summary>Moderation settings (implemented in Infrastructure from the "Moderation" settings).</summary>
public interface IModerationSettingsProvider
{
    /// <summary>True = every dish and shop name goes to a person, whatever the AI says (busy or risky times).</summary>
    bool HoldAll { get; }
}
