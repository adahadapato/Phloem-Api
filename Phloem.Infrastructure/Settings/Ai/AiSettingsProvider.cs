using Microsoft.Extensions.Options;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;

namespace Phloem.Infrastructure.Settings.Ai;

/// <summary>Implements <see cref="IAiSettingsProvider"/> from <see cref="AiSettingsOptions"/> (never the key).</summary>
internal sealed class AiSettingsProvider(IOptions<AiSettingsOptions> options) : IAiSettingsProvider
{
    /// <inheritdoc />
    public AiSettings Get()
    {
        var o = options.Value;
        var reason = o.NotReadyReason();
        return new AiSettings(o.Provider, o.Provider == AiProvider.Fake ? "fake (sample answers)" : o.Model,
                              reason is null, reason, o.MonthlyCapUsd, o.LogRetentionMonths);
    }
}
