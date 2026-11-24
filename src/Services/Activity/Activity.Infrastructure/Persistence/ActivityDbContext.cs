using Activity.Infrastructure.EventStore;
using Activity.Infrastructure.Projections;
using Activity.Infrastructure.Projections.Stats;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.Persistence;

public class ActivityDbContext(DbContextOptions<ActivityDbContext> options) : DbContext(options)
{
    /// <summary>Все события сервиса — источник правды.</summary>
    public DbSet<EventRecord> Events => Set<EventRecord>();

    /// <summary>Проекция "мои списки" — итог событий для быстрого чтения.</summary>
    public DbSet<LibraryEntryRow> LibraryEntries => Set<LibraryEntryRow>();

    public DbSet<DailyReadingStatsRow> DailyReadingStatsRows =>
        Set<DailyReadingStatsRow>();

    public DbSet<DailyActiveReaderRow> DailyActiveReaderRows =>
        Set<DailyActiveReaderRow>();

    public DbSet<MangaReadingStatsRow> MangaReadingStats =>
        Set<MangaReadingStatsRow>();

    public DbSet<MangaCompletedReaderRow> MangaCompletedReaderRows =>
        Set<MangaCompletedReaderRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActivityDbContext).Assembly);
}
