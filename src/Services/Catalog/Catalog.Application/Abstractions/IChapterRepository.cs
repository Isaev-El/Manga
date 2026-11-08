using Catalog.Domain;

namespace Catalog.Application.Abstractions;

public interface IChapterRepository
{
    Task<Chapter?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Chapter>> ListByMangaAsync(Guid mangaId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid mangaId, decimal number, CancellationToken ct = default);
    void Add(Chapter chapter); // без сохранения: сохраняет Unit of Work
}
