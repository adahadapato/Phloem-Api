using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Domain.Interfaces;

namespace Phloem.Infrastructure.Settings.Email;

/// <summary>Registers email sending. Called from <see cref="DependencyInjection.AddInfrastructure"/>.</summary>
internal static class EmailSetup
{
    /// <summary>
    /// Binds and checks <see cref="EmailSettings"/> (including the nested <see cref="ReplyToSettings"/>) when the
    /// app starts, and registers <see cref="IEmailSender"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration (appsettings, secrets, environment variables).</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EmailSettings>()
                .Bind(configuration.GetSection(EmailSettings.SectionName))
                .ValidateDataAnnotations()
                .Validate(settings => IsValid(settings.ReplyTo),
                          "Email:ReplyTo needs valid Support, Sellers and Shipping addresses.")
                .ValidateOnStart();

        services.AddTransient<IEmailSender, SmtpEmailSender>();
        return services;
    }

    /// <summary>Runs the data-annotation checks on the nested reply-to settings.</summary>
    /// <param name="replyTo">The nested settings.</param>
    /// <returns>True when every address is present and valid.</returns>
    private static bool IsValid(ReplyToSettings? replyTo) =>
        replyTo is not null &&
        Validator.TryValidateObject(replyTo, new ValidationContext(replyTo), null, validateAllProperties: true);
}