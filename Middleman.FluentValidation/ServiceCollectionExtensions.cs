using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace Middleman.FluentValidation
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMiddlemanFluentValidation(this IServiceCollection services, Assembly assembly)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<>), typeof(ValidationBehaviorNoResult<>));
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
