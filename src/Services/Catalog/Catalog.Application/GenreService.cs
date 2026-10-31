using Catalog.Application.Abstractions;

namespace Catalog.Application;

public class GenreService(IGenreRepository genres)
{
    public async Task<IReadOnlyList<GenreDto>> ListAsync(CancellationToken ct)
    {
        var items = await genres.ListAsync(ct);
        return items.Select(g => g.ToDto()).ToList();
    }
}
