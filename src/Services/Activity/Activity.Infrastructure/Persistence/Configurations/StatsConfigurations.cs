using Activity.Infrastructure.Projections.Stats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activity.Infrastructure.Persistence.Configurations;

public class DailyReadingStatsRowConfiguration
    : IEntityTypeConfiguration<DailyReadingStatsRow>
{
    public void Configure(EntityTypeBuilder<DailyReadingStatsRow> builder)
    {
        builder.ToTable("daily_reading_stats");
        builder.HasKey(x => x.Day);
    }
}

public class DailyActiveReaderRowConfiguration
    : IEntityTypeConfiguration<DailyActiveReaderRow>
{
    public void Configure(EntityTypeBuilder<DailyActiveReaderRow> builder)
    {
        builder.ToTable("daily_active_readers");
        builder.HasKey(x => new { x.Day, x.UserId });
        builder.Property(x => x.UserId).HasMaxLength(100);
    }
}

public class MangaReadingStatsRowConfiguration
    : IEntityTypeConfiguration<MangaReadingStatsRow>
{
    public void Configure(EntityTypeBuilder<MangaReadingStatsRow> builder)
    {
        builder.ToTable("manga_reading_stats");
        builder.HasKey(x => x.MangaId);
        builder.HasIndex(x => x.ChaptersRead);
    }
}

public class MangaCompletedReaderRowConfiguration
    : IEntityTypeConfiguration<MangaCompletedReaderRow>
{
    public void Configure(EntityTypeBuilder<MangaCompletedReaderRow> builder)
    {
        builder.ToTable("manga_completed_readers");
        builder.HasKey(x => new { x.MangaId, x.UserId });
        builder.Property(x => x.UserId).HasMaxLength(100);
    }
}
