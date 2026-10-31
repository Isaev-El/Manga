using Catalog.Domain;

namespace Catalog.Application.Abstractions;

public interface IGenreRepository
{
    Task<IReadOnlyList<Genre>> ListAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
