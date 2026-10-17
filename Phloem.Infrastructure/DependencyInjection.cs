using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Infrastructure.Settings.Application;
using Phloem.Infrastructure.Settings.Email;
using Phloem.Infrastructure.Settings.Storage;

namespace Phloem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationSettings(configuration);
            services.AddEmail(configuration);
            services.AddStorage(configuration);
            // services.AddImages();                       next
            // services.AddPhloemIdentity(configuration);  after Persistence

            // Add infrastructure layer services here




            return services;
        }
    }
}
