using Catalog.Application.Abstractions;
using MediatR;

namespace Catalog.Application.Features.Mangas;

// Запрос (query): только читает данные, ничего не меняет
public record GetMangaQuery(Guid MangaId) : IRequest<MangaDto>;

public class GetMangaQueryHandler(IMangaRepository mangas) : IRequestHandler<GetMangaQuery, MangaDto>
{
    public async Task<MangaDto> Handle(GetMangaQuery query, CancellationToken ct)
    {
        var manga = await mangas.GetByIdAsync(query.MangaId, ct)
            ?? throw new NotFoundException($"Тайтл {query.MangaId} не найден");

        return manga.ToDto();
    }
}
