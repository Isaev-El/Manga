using Activity.Application;
using Activity.Application.Abstractions;
using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.ReadModel;

public class ReadingStatsReadModel(ActivityDbContext db) : IReadingStatsReadModel
{
    public async Task<IReadOnlyList<DailyReadingStatsDto>> GetReadingStatsAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var rows = await db.DailyReadingStatsRows
            .AsNoTracking()
            .Where(x => x.Day >= from && x.Day <= to)
            .ToDictionaryAsync(x => x.Day, ct);

        var result = new List<DailyReadingStatsDto>();

        for (var day = from; day <= to; day = day.AddDays(1))
        {
            result.Add(rows.TryGetValue(day, out var row)
                ? new DailyReadingStatsDto(
                    row.Day,
                    row.ChaptersRead,
                    row.ActiveReaders,
                    row.TitlesAdded)
                : new DailyReadingStatsDto(day, 0, 0, 0));
        }
        return result;
    }

    public async Task<IReadOnlyList<TopMangaDto>> GetTopMangaAsync(
        int limit, CancellationToken ct = default)
    {
        return await db.MangaReadingStats
            .AsNoTracking()
            .OrderByDescending(x => x.ChaptersRead)
            .ThenBy(x => x.MangaId)
            .Take(limit)
            .Select(x => new TopMangaDto(
                x.MangaId,
                x.ChaptersRead,
                x.InLists,
                x.Completed))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DropOffDto>> GetDropOffAsync(
        Guid mangaId, CancellationToken ct = default)
    {
        var rows = await db.LibraryEntries
            .AsNoTracking()
            .Where(x => x.MangaId == mangaId
                        && x.LastReadChapterNumber > 0)
            .GroupBy(x => x.LastReadChapterNumber)
            .Select(g => new
            {
                ChapterNumber = g.Key,
                ReadersStopped = g.Count()
            })
            .OrderBy(x => x.ChapterNumber)
            .ToListAsync(ct);

        return rows
            .Select(x => new DropOffDto(x.ChapterNumber, x.ReadersStopped))
            .ToList();
    }
}
