using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.ToTable("chapters");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Number).HasPrecision(8, 2);
        builder.Property(c => c.Title).HasMaxLength(200);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(c => c.DomainEvents);

        // Внутри одного сервиса внешний ключ на мангу допустим.
        // Restrict: мангу с главами удалить нельзя.
        builder.HasOne<Manga>()
            .WithMany()
            .HasForeignKey(c => c.MangaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Номер главы уникален в пределах тайтла
        builder.HasIndex(c => new { c.MangaId, c.Number }).IsUnique();

        // Страницы принадлежат главе и хранятся в своей таблице
        builder.OwnsMany(c => c.Pages, pages =>
        {
            pages.ToTable("chapter_pages");
            pages.WithOwner().HasForeignKey("ChapterId");
            pages.HasKey(p => p.Id);
            pages.Property(p => p.Id).ValueGeneratedNever();
            pages.Property(p => p.ImageKey).HasMaxLength(500).IsRequired();
        });

        builder.Navigation(c => c.Pages).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
