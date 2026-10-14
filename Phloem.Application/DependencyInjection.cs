using Microsoft.Extensions.DependencyInjection;

namespace Phloem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Add application layer services here
            return services;
        }
    }
}
