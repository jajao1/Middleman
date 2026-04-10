using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Middleman
{
    /// <summary>
    /// Provides extension methods for setting up Middleman services in an IServiceCollection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scans the specified assembly for Middleman handlers and registers them in the IServiceCollection.
        /// It also registers the core Middleman services.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="assemblyToScan">The assembly to scan for handlers.</param>
        /// <returns>The same IServiceCollection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddMiddleman(this IServiceCollection services, Assembly assemblyToScan)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assemblyToScan);

            return AddMiddleman(services, assemblyToScan, configureOptions: null);
        }

        /// <summary>
        /// Scans the specified assembly for Middleman handlers and registers them in the IServiceCollection.
        /// It also registers the core Middleman services and allows options customization.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="assemblyToScan">The assembly to scan for handlers.</param>
        /// <param name="configureOptions">Optional callback to configure Middleman options.</param>
        /// <returns>The same IServiceCollection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddMiddleman(
            this IServiceCollection services,
            Assembly assemblyToScan,
            Action<MiddlemanOptions>? configureOptions)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assemblyToScan);

            var options = new MiddlemanOptions();
            configureOptions?.Invoke(options);

            services.AddSingleton(options);

            // 1. Register the main Middleman dispatcher implementation.
            // A transient lifetime is appropriate as it holds no state.
            services.AddTransient<IMiddleman, Middleman>();

            // 1.1 Register built-in exception handling behaviors for both request forms.
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<>), typeof(ExceptionHandlingBehavior<>));

            // 2. Scan and register all IRequestHandler<TMessage, TResponse> implementations (with return value).
            RegisterHandlers(services, typeof(IRequestHandler<,>), assemblyToScan);

            // 3. Scan and register all IRequestHandler<TMessage> implementations (without return value).
            RegisterHandlers(services, typeof(IRequestHandler<>), assemblyToScan);

            // 4. Scan and register all INotificationHandler<TNotification> implementations.
            RegisterHandlers(services, typeof(INotificationHandler<>), assemblyToScan);

            // 5. Scan and register all stream handlers.
            RegisterHandlers(services, typeof(IStreamRequestHandler<,>), assemblyToScan);

            // 6. Scan and register all exception handlers.
            RegisterHandlers(services, typeof(IExceptionHandler<,>), assemblyToScan);
            RegisterHandlers(services, typeof(IExceptionHandler<>), assemblyToScan);

            return services;

        }

        private static void RegisterHandlers(IServiceCollection services, Type handlerInterface, Assembly assembly)
        {
            foreach (var handlerType in assembly.GetTypes().Where(t =>
                         t is { IsClass: true, IsAbstract: false } &&
                         t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)))
            {
                foreach (var serviceType in handlerType.GetInterfaces()
                             .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface))
                {
                    services.AddTransient(serviceType, handlerType);
                }
            }
        }
    }
}
