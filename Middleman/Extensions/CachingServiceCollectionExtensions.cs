using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Middleman
{
    public static class CachingServiceCollectionExtensions
    {
        /// <summary>
        /// Enables response caching for cacheable requests.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configureCache">Optional MemoryCache configuration callback.</param>
        /// <returns>The same service collection for chaining.</returns>
        public static IServiceCollection AddMiddlemanCaching(
            this IServiceCollection services,
            Action<MemoryCacheOptions>? configureCache = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            if (configureCache is null)
            {
                services.AddMemoryCache();
            }
            else
            {
                services.AddMemoryCache(configureCache);
            }

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

            return services;
        }
    }
}
