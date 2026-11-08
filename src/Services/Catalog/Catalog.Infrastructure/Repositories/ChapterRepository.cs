using Catalog.Application.Abstractions;
using Catalog.Domain;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public class ChapterRepository(CatalogDbContext db) : IChapterRepository
{
    // Страницы — owned-сущности, EF Core загружает их вместе с главой автоматически
    public Task<Chapter?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Chapters.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Chapter>> ListByMangaAsync(Guid mangaId, CancellationToken ct = default) =>
        await db.Chapters
            .AsNoTracking()
            .Where(c => c.MangaId == mangaId)
            .OrderBy(c => c.Number)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(Guid mangaId, decimal number, CancellationToken ct = default) =>
        db.Chapters.AnyAsync(c => c.MangaId == mangaId && c.Number == number, ct);

    public void Add(Chapter chapter) => db.Chapters.Add(chapter);
}
