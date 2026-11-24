using Activity.Application.Abstractions;
using MediatR;

namespace Activity.Application.Features.Ratings;

/// <summary>Средняя оценка тайтла. Отдельная проекция не нужна: это AVG по library_entries.</summary>
public record GetMangaRatingQuery(Guid MangaId) : IRequest<MangaRatingDto>;

public class GetMangaRatingQueryHandler(ILibraryReadModel readModel)
    : IRequestHandler<GetMangaRatingQuery, MangaRatingDto>
{
    public Task<MangaRatingDto> Handle(GetMangaRatingQuery query, CancellationToken ct) =>
        readModel.GetMangaRatingAsync(query.MangaId, ct);
}
