using Catalog.Application.Abstractions;
using MediatR;

namespace Catalog.Application.Features.Mangas;

public record ListMangaQuery(Guid? GenreId) : IRequest<IReadOnlyList<MangaDto>>;

public class ListMangaQueryHandler(IMangaRepository mangas)
    : IRequestHandler<ListMangaQuery, IReadOnlyList<MangaDto>>
{
    public async Task<IReadOnlyList<MangaDto>> Handle(ListMangaQuery query, CancellationToken ct)
    {
        var items = await mangas.ListAsync(query.GenreId, ct);
        return items.Select(m => m.ToDto()).ToList();
    }
}
