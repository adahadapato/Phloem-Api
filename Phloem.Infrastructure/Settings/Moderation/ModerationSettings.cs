using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Infrastructure.Services.Moderation;

namespace Phloem.Infrastructure.Settings.Moderation;

/// <summary>The "Moderation" settings (see <see cref="AppSettings.Moderation"/>).</summary>
public sealed class ModerationSettingsOptions
{
    /// <summary>The section name.</summary>
    public const string SectionName = AppSettings.Moderation.Section;

    /// <summary>True = every dish and live shop name goes to a person, whatever the AI says.</summary>
    public bool HoldAll { get; set; }
}

/// <summary>Implements <see cref="IModerationSettingsProvider"/>; read live, so a settings change applies without a
/// restart where the host reloads settings.</summary>
internal sealed class ModerationSettingsProvider(IOptionsMonitor<ModerationSettingsOptions> options)
    : IModerationSettingsProvider
{
    /// <inheritdoc />
    public bool HoldAll => options.CurrentValue.HoldAll;
}

/// <summary>Registers moderation: settings and the background checker.</summary>
internal static class ModerationSetup
{
    /// <summary>Binds the "Moderation" section and starts the checker.</summary>
    public static IServiceCollection AddModeration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ModerationSettingsOptions>()
                .Bind(configuration.GetSection(ModerationSettingsOptions.SectionName));
        services.AddSingleton<IModerationSettingsProvider, ModerationSettingsProvider>();
        services.AddHostedService<ModerationWorker>();   // every minute: check new text with the AI
        return services;
    }
}
