using Catalog.Application.Abstractions;
using MediatR;

namespace Catalog.Application.Features.Chapters;

public record ListChaptersQuery(Guid MangaId) : IRequest<IReadOnlyList<ChapterDto>>;

public class ListChaptersQueryHandler(IChapterRepository chapters, IMangaRepository mangas)
    : IRequestHandler<ListChaptersQuery, IReadOnlyList<ChapterDto>>
{
    public async Task<IReadOnlyList<ChapterDto>> Handle(ListChaptersQuery query, CancellationToken ct)
    {
        if (!await mangas.ExistsAsync(query.MangaId, ct))
            throw new NotFoundException($"Тайтл {query.MangaId} не найден");

        var items = await chapters.ListByMangaAsync(query.MangaId, ct);
        return items.Select(c => c.ToDto()).ToList();
    }
}
