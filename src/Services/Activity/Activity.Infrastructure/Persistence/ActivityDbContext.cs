using Activity.Infrastructure.EventStore;
using Activity.Infrastructure.Projections;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.Persistence;

public class ActivityDbContext(DbContextOptions<ActivityDbContext> options) : DbContext(options)
{
    /// <summary>Все события сервиса — источник правды.</summary>
    public DbSet<EventRecord> Events => Set<EventRecord>();

    /// <summary>Проекция "мои списки" — итог событий для быстрого чтения.</summary>
    public DbSet<LibraryEntryRow> LibraryEntries => Set<LibraryEntryRow>();

    /// <summary>Проекция комментариев к главам.</summary>
    public DbSet<ChapterCommentRow> ChapterComments => Set<ChapterCommentRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActivityDbContext).Assembly);
}
