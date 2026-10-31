using Catalog.Application.Abstractions;
using Catalog.Domain;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public class GenreRepository(CatalogDbContext db) : IGenreRepository
{
    public async Task<IReadOnlyList<Genre>> ListAsync(CancellationToken ct = default) =>
        await db.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Genre>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var list = ids.Distinct().ToList();
        return await db.Genres
            .Where(g => list.Contains(g.Id))
            .ToListAsync(ct);
    }
}
