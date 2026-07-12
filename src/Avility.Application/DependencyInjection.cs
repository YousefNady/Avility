using System.Reflection;
using Avility.Application.Common.Behaviors;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Avility.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Registration order = wrapping order: Logging runs first and
        // last (outermost), Performance wraps only the actual handler
        // execution (innermost) - so a validation failure is logged but
        // not counted as a "slow request".
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        services.AddScoped<Auth.TokenIssuer>();
        
        services.AddScoped<IJobApplicationAccessGuard, JobApplicationAccessGuard>();

        return services;
    }
}