using System.Text.Json;
using Activity.Application;
using Activity.Application.Abstractions;
using Activity.Domain;
using Activity.Domain.Events;
using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.ReadModel;

/// <summary>Сторона чтения: запросы к проекции и к истории событий.</summary>
public class LibraryReadModel(ActivityDbContext db) : ILibraryReadModel
{
    public async Task<IReadOnlyList<LibraryEntryDto>> GetLibraryAsync(
        string userId, ListStatus? status, CancellationToken ct = default)
    {
        var query = db.LibraryEntries
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status != null); // убранные из списков не показываем

        if (status is not null)
            query = query.Where(x => x.Status == status);

        var rows = await query.OrderByDescending(x => x.UpdatedAtUtc).ToListAsync(ct);

        return rows
            .Select(x => new LibraryEntryDto(
                x.MangaId, x.Status!.Value.ToString(), x.LastReadChapterId,
                x.LastReadChapterNumber, x.Rating, x.UpdatedAtUtc))
            .ToList();
    }

    public async Task<IReadOnlyList<HistoryItemDto>> GetHistoryAsync(
        string userId, Guid mangaId, CancellationToken ct = default)
    {
        var streamId = ReadingEntry.StreamId(userId, mangaId);

        var records = await db.Events
            .AsNoTracking()
            .Where(e => e.StreamId == streamId)
            .OrderBy(e => e.Version)
            .ToListAsync(ct);

        return records
            .Select(r =>
            {
                using var doc = JsonDocument.Parse(r.Data);
                return new HistoryItemDto(r.Version, r.Type, doc.RootElement.Clone(), r.OccurredOnUtc);
            })
            .ToList();
    }

    public async Task<MangaRatingDto> GetMangaRatingAsync(Guid mangaId, CancellationToken ct = default)
    {
        // Отдельная проекция не нужна: средняя оценка — обычный AVG по library_entries
        var ratings = db.LibraryEntries
            .AsNoTracking()
            .Where(x => x.MangaId == mangaId && x.Status != null && x.Rating != null)
            .Select(x => x.Rating!.Value);

        var votes = await ratings.CountAsync(ct);
        double? average = votes == 0 ? null : Math.Round(await ratings.AverageAsync(ct), 2);

        return new MangaRatingDto(mangaId, average, votes);
    }
}
