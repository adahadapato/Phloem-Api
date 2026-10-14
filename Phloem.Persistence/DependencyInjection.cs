using Microsoft.Extensions.DependencyInjection;

namespace Phloem.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            // Add persistence layer services here
            return services;
        }
    }
}
