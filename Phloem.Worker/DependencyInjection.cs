using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Phloem.Worker
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWorker(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
