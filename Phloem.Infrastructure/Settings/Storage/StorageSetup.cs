using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Phloem.Infrastructure.Settings.Storage;

/// <summary>Registers file storage. Called from <see cref="DependencyInjection.AddInfrastructure"/>.</summary>
internal static class StorageSetup
{
    /// <summary>
    /// Binds and checks <see cref="StorageSettings"/> at start-up, and registers <see cref="IFileStorage"/> for the
    /// configured <see cref="StorageProvider"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="InvalidOperationException">A provider that has no implementation yet.</exception>
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<StorageSettings>()
                .Bind(configuration.GetSection(StorageSettings.SectionName))
                .Validate(s => s.Provider != StorageProvider.Local || IsValid(s.Local),
                          "Storage:Local needs PublicRootPath, PrivateRootPath, PrivateDownloadPath and a SigningKey " +
                          $"of at least {LocalStorageSettings.MinSigningKeyLength} characters (set the key in User Secrets).")
                .ValidateOnStart();

        var provider = configuration.GetValue<StorageProvider?>(AppSettings.Storage.Provider) ?? StorageProvider.Local;

        switch (provider)
        {
            case StorageProvider.Local:
                services.AddSingleton<LocalStoragePaths>();
                services.AddSingleton<PrivateFileLinks>();
                services.AddSingleton<IFileStorage, LocalFileStorage>();
                break;

            default:
                throw new InvalidOperationException($"Storage provider '{provider}' isn't supported yet.");
        }

        return services;
    }

    /// <summary>Checks the local settings (data annotations + key length).</summary>
    private static bool IsValid(LocalStorageSettings? local) =>
        local is not null &&
        Validator.TryValidateObject(local, new ValidationContext(local), null, validateAllProperties: true) &&
        local.SigningKey.Length >= LocalStorageSettings.MinSigningKeyLength;
}
