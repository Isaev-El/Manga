using Activity.Application.Abstractions;
using Activity.Infrastructure.Catalog;
using Activity.Infrastructure.Persistence;
using Activity.Infrastructure.Projections;
using Activity.Infrastructure.Projections.Stats;
using Activity.Infrastructure.ReadModel;
using Activity.Infrastructure.Rebuild;
using Activity.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Activity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string catalogUrl)
    {
        services.AddDbContext<ActivityDbContext>(
            options => options.UseNpgsql(connectionString));

        services.AddScoped<IReadingEntryRepository, ReadingEntryRepository>();
        services.AddScoped<ILibraryReadModel, LibraryReadModel>();
        services.AddScoped<IReadingStatsReadModel, ReadingStatsReadModel>();
        services.AddScoped<IStatsProjectionRebuilder, StatsProjectionRebuilder>();

        // Library-проекция: участвует в обычном сохранении,
        // но не в пересборке статистики.
        services.AddScoped<LibraryEntryProjection>();
        services.AddScoped<IProjection>(
            sp => sp.GetRequiredService<LibraryEntryProjection>());

        // Stats-проекции: участвуют и в обычном сохранении, и в пересборке.
        services.AddScoped<DailyReadingStatsProjection>();
        services.AddScoped<IProjection>(
            sp => sp.GetRequiredService<DailyReadingStatsProjection>());
        services.AddScoped<IStatsProjection>(
            sp => sp.GetRequiredService<DailyReadingStatsProjection>());

        services.AddScoped<MangaReadingStatsProjection>();
        services.AddScoped<IProjection>(
            sp => sp.GetRequiredService<MangaReadingStatsProjection>());
        services.AddScoped<IStatsProjection>(
            sp => sp.GetRequiredService<MangaReadingStatsProjection>());

        services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
        {
            client.BaseAddress = new Uri(catalogUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
