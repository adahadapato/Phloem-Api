using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Domain.Interfaces;

namespace Phloem.Infrastructure.Settings.Contact;

/// <summary>Registers the Contact us settings. Called from <see cref="DependencyInjection.AddInfrastructure"/>.</summary>
internal static class ContactSetup
{
    /// <summary>Binds and checks the "Contact" section when the app starts, and registers <see cref="IContactDetailsProvider"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddContact(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ContactSettings>()
                .Bind(configuration.GetSection(ContactSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<IContactDetailsProvider, ContactDetailsProvider>();
        return services;
    }
}
