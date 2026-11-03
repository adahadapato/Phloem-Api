using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Domain.Interfaces;
using Phloem.Infrastructure.Services.Ai;

namespace Phloem.Infrastructure.Settings.Ai;

/// <summary>Registers everything AI: settings, the model connection, prompts, the JSON client and the log clean-up.</summary>
internal static class AiSetup
{
    /// <summary>Binds and checks the "Ai" section when the app starts, and registers the AI services.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiSettingsOptions>()
                .Bind(configuration.GetSection(AiSettingsOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<IAiSettingsProvider, AiSettingsProvider>();
        services.AddSingleton<AiModel>();
        services.AddSingleton<IAiPromptProvider, AiPromptProvider>();
        services.AddScoped<IAiJsonClient, AiJsonClient>();

        services.AddHostedService<AiCallLogCleanupService>();   // daily: delete AI calls past the keeping period
        return services;
    }
}
