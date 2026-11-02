using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Domain.Interfaces;

namespace Phloem.Infrastructure.Settings.Approvals;

/// <summary>Registers the approval settings.</summary>
internal static class ApprovalSetup
{
    /// <summary>Binds and checks the "Approvals" section when the app starts, and registers
    /// <see cref="IApprovalSettingsProvider"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddApprovals(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApprovalSettingsOptions>()
                .Bind(configuration.GetSection(ApprovalSettingsOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<IApprovalSettingsProvider, ApprovalSettingsProvider>();
        return services;
    }
}
