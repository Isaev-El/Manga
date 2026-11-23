using Activity.Application.Abstractions;
using Activity.Infrastructure.Persistence;
using Activity.Infrastructure.Projections;
using Activity.Infrastructure.Projections.Stats;
using Activity.Infrastructure.ReadModel;
using Activity.Infrastructure.Rebuild;
using Activity.Infrastructure.Repositories;

namespace Activity.Tests;

public static class TestServices
{
    public static ReadingEntryRepository CreateRepository(ActivityDbContext db) =>
        new(db, new IProjection[]
        {
            new LibraryEntryProjection(db),
            new DailyReadingStatsProjection(db),
            new MangaReadingStatsProjection(db)
        });

    public static ReadingStatsReadModel CreateReadModel(ActivityDbContext db) =>
        new(db);

    public static StatsProjectionRebuilder CreateRebuilder(ActivityDbContext db) =>
        new(db, new IStatsProjection[]
        {
            new DailyReadingStatsProjection(db),
            new MangaReadingStatsProjection(db)
        });
}
