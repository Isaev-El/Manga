using Activity.Infrastructure.Projections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activity.Infrastructure.Persistence.Configurations;

public class LibraryEntryRowConfiguration : IEntityTypeConfiguration<LibraryEntryRow>
{
    public void Configure(EntityTypeBuilder<LibraryEntryRow> builder)
    {
        builder.ToTable("library_entries");
        builder.HasKey(x => new { x.UserId, x.MangaId });

        builder.Property(x => x.UserId).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.LastReadChapterNumber).HasPrecision(8, 2);

        // Для средней оценки тайтла по всем пользователям
        builder.HasIndex(x => x.MangaId);
    }
}
