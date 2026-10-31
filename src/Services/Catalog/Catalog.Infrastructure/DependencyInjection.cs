using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));

        // Всё Scoped: в пределах одного запроса UnitOfWork и репозитории
        // получают один и тот же DbContext, поэтому UoW видит все изменения.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMangaRepository, MangaRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();

        return services;
    }
}
