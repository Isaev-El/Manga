namespace Activity.Application.Abstractions;

public interface IReadingStatsReadModel
{
    Task<IReadOnlyList<DailyReadingStatsDto>> GetReadingStatsAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<IReadOnlyList<TopMangaDto>> GetTopMangaAsync(
        int limit, CancellationToken ct = default);

    Task<IReadOnlyList<DropOffDto>> GetDropOffAsync(
        Guid mangaId, CancellationToken ct = default);
}
