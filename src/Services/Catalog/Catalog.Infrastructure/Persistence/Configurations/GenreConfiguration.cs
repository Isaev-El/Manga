using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("genres");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();
        builder.Property(g => g.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(g => g.Name).IsUnique();
        builder.Ignore(g => g.DomainEvents);

        // Начальные данные: попадают в миграцию, жанры есть в базе сразу после запуска
        builder.HasData(
            new { Id = SeedGenres.Shonen, Name = "Сёнэн" },
            new { Id = SeedGenres.Seinen, Name = "Сэйнэн" },
            new { Id = SeedGenres.Fantasy, Name = "Фэнтези" },
            new { Id = SeedGenres.Drama, Name = "Драма" },
            new { Id = SeedGenres.Comedy, Name = "Комедия" });
    }
}

public static class SeedGenres
{
    public static readonly Guid Shonen = Guid.Parse("11111111-0000-0000-0000-000000000001");
    public static readonly Guid Seinen = Guid.Parse("11111111-0000-0000-0000-000000000002");
    public static readonly Guid Fantasy = Guid.Parse("11111111-0000-0000-0000-000000000003");
    public static readonly Guid Drama = Guid.Parse("11111111-0000-0000-0000-000000000004");
    public static readonly Guid Comedy = Guid.Parse("11111111-0000-0000-0000-000000000005");
}
