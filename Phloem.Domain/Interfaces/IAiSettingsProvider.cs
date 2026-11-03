using Phloem.Domain.Models;

namespace Phloem.Domain.Interfaces;

/// <summary>Reads <see cref="AiSettings"/> (implemented in Infrastructure from the "Ai" settings; never the key).</summary>
public interface IAiSettingsProvider
{
    /// <summary>The current AI settings.</summary>
    AiSettings Get();
}
