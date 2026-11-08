using Catalog.Domain;

namespace Catalog.Application.Abstractions;

public interface IMangaRepository
{
    Task<Manga?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Manga>> ListAsync(Guid? genreId, CancellationToken ct = default);
    void Add(Manga manga); // без сохранения: сохраняет Unit of Work
}
