using Activity.Application.Abstractions;
using Activity.Infrastructure.Catalog;
using Activity.Infrastructure.Persistence;
using Activity.Infrastructure.ReadModel;
using Activity.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Activity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string catalogUrl)
    {
        services.AddDbContext<ActivityDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IReadingEntryRepository, ReadingEntryRepository>();
        services.AddScoped<ILibraryReadModel, LibraryReadModel>();

        services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
        {
            client.BaseAddress = new Uri(catalogUrl); // слэш в конце адреса обязателен
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
