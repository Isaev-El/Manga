using Activity.Domain;

namespace Activity.Application.Abstractions;

/// <summary>
/// Сторона чтения (CQRS): запросы читают готовые проекции и историю событий,
/// не восстанавливая агрегаты.
/// </summary>
public interface ILibraryReadModel
{
    Task<IReadOnlyList<LibraryEntryDto>> GetLibraryAsync(string userId, ListStatus? status, CancellationToken ct = default);
    Task<IReadOnlyList<HistoryItemDto>> GetHistoryAsync(string userId, Guid mangaId, CancellationToken ct = default);
    Task<MangaRatingDto> GetMangaRatingAsync(Guid mangaId, CancellationToken ct = default);
}
