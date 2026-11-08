using Catalog.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            // находит все IRequestHandler<,> в этом проекте
            cfg.RegisterServicesFromAssembly(assembly);

            // каждый запрос перед обработчиком проходит через валидацию
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // находит все AbstractValidator<T> в этом проекте
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
