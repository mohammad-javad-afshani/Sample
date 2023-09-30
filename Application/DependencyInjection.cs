using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembely = typeof(DependencyInjection).Assembly;

            services.AddMediatR(configuration =>
                configuration.RegisterServicesFromAssembly(assembely));

            services.AddValidatorsFromAssembly(assembely);



            return services;
        }
    }
}

