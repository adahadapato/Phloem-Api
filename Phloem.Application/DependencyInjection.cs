using Microsoft.Extensions.DependencyInjection;
using Phloem.Application.Common.Behaviors;
using Phloem.Application.Common.Behaviours;

namespace Phloem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            // The clock: .NET's built-in TimeProvider (FakeTimeProvider in tests).
            services.AddSingleton(TimeProvider.System);


            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                // Outermost first: Logging → Validation → Caching → Cache invalidation → handler
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
                cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
                // v13+ only: cfg.LicenseKey = "...";
            });

            // Add application layer services here
            return services;
        }
    }
}
