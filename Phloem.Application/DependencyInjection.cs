using FluentValidation;
using Phloem.Application.Common.Terms;
using Phloem.Application.Features.Deliveries.Common;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Application.Common.Behaviors;
using Phloem.Application.Common.Behaviours;

namespace Phloem.Application
{
    /// <summary>
    /// Registers everything Application provides:
    /// <list type="number">
    ///   <item><description><b>Clock:</b> .NET's <see cref="TimeProvider"/> (a fake one in tests).</description></item>
    ///   <item><description><b>Cache:</b> <c>HybridCache</c>, used by the caching behaviors.</description></item>
    ///   <item><description><b>Validators:</b> every FluentValidation validator in this project.</description></item>
    ///   <item><description><b>MediatR:</b> handlers and the pipeline behaviors.</description></item>
    /// </list>
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>Adds the Application layer's services.</summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection, for chaining.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            // 1. The clock: .NET's built-in TimeProvider (FakeTimeProvider in tests)
            services.AddSingleton(TimeProvider.System);

            // 2. Cache: in-memory by default; a shared cache (e.g. Redis) can be added later without code changes here
            services.AddHybridCache();

            // 2b. Terms: "has accepted the latest Terms?" and "record an acceptance", for the handlers
            services.AddScoped<TermsGate>();

            // 2c. Deliveries: who can be a courier, proof-of-delivery photos, delivery emails
            services.AddScoped<CourierDirectory>();
            services.AddScoped<DeliveryPhotos>();
            services.AddScoped<DeliveryEmails>();

            // 3. Validators: without this, ValidationBehavior finds none and NO validation runs
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

            // 4. MediatR: outermost first: Logging → Validation → Caching → Cache invalidation → handler
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
                cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
                // v13+ only: cfg.LicenseKey = "...";
            });

            return services;
        }
    }
}