using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Application;
using Phloem.Infrastructure.Services.Accounts;
using Phloem.Infrastructure.Settings.Application;
using Phloem.Infrastructure.Settings.Email;
using Phloem.Infrastructure.Settings.Identity;
using Phloem.Infrastructure.Settings.Images;
using Phloem.Infrastructure.Settings.Storage;
using Phloem.Persistence;

namespace Phloem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplication();                   // MediatR pipeline, validators, caching, TimeProvider
            services.AddPersistence(configuration);      // SQL Server DbContext, unit of work, repositories

            services.AddApplicationSettings(configuration);
            services.AddEmail(configuration);
            services.AddStorage(configuration);
            services.AddImages(configuration);                     
            services.AddPhloemIdentity(configuration);  

            // Background tasks
            services.AddHostedService<SuspensionExpiryService>();   // hourly: lift suspensions that have ended

            // Add infrastructure layer services here




            return services;
        }
    }
}
