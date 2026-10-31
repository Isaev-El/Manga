using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public class MangaConfiguration : IEntityTypeConfiguration<Manga>
{
    public void Configure(EntityTypeBuilder<Manga> builder)
    {
        builder.ToTable("mangas");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Author).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(m => m.DomainEvents); // события в таблицу сущности не пишем

        // Многие-ко-многим через таблицу manga_genres
        builder.HasMany(m => m.Genres)
            .WithMany()
            .UsingEntity("manga_genres");

        builder.Navigation(m => m.Genres).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
