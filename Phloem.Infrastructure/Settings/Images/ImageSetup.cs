using ImageMagick;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Domain.Interfaces;
using Phloem.Domain.ValueObjects;

namespace Phloem.Infrastructure.Settings.Images;

/// <summary>Registers the image pipeline. Called from <see cref="DependencyInjection.AddInfrastructure"/>.</summary>
internal static class ImageSetup
{
    /// <summary>Most memory ImageMagick may use for one image before refusing it (256 MB).</summary>
    private const ulong MaxMemoryBytes = 256UL * 1024 * 1024;

    /// <summary>
    /// Binds and checks <see cref="ImageSettings"/> at start-up, sets ImageMagick's safety limits once for the
    /// whole app, and registers <see cref="IImageProcessor"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddImages(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ImageSettings>()
                .Bind(configuration.GetSection(ImageSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        // Process-wide safety limits: anything bigger is refused before it can use the server's memory
        ResourceLimits.Width = ImageDimensions.MaxSide;
        ResourceLimits.Height = ImageDimensions.MaxSide;
        ResourceLimits.Memory = MaxMemoryBytes;

        services.AddSingleton<IImageProcessor, MagickImageProcessor>();
        return services;
    }
}