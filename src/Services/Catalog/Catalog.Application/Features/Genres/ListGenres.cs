using Catalog.Application.Abstractions;
using MediatR;

namespace Catalog.Application.Features.Genres;

public record ListGenresQuery : IRequest<IReadOnlyList<GenreDto>>;

public class ListGenresQueryHandler(IGenreRepository genres)
    : IRequestHandler<ListGenresQuery, IReadOnlyList<GenreDto>>
{
    public async Task<IReadOnlyList<GenreDto>> Handle(ListGenresQuery query, CancellationToken ct)
    {
        var items = await genres.ListAsync(ct);
        return items.Select(g => g.ToDto()).ToList();
    }
}
