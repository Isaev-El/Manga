using Catalog.Application.Abstractions;
using Catalog.Domain;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public class MangaRepository(CatalogDbContext db) : IMangaRepository
{
    public Task<Manga?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Mangas
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Manga>> ListAsync(Guid? genreId, CancellationToken ct = default)
    {
        IQueryable<Manga> query = db.Mangas
            .AsNoTracking()
            .Include(m => m.Genres);

        if (genreId is not null)
            query = query.Where(m => m.Genres.Any(g => g.Id == genreId));

        return await query.OrderBy(m => m.Title).ToListAsync(ct);
    }

    public void Add(Manga manga) => db.Mangas.Add(manga);
}
