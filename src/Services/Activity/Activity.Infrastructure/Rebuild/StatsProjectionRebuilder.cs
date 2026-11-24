using Activity.Application;
using Activity.Application.Abstractions;
using Activity.Infrastructure.EventStore;
using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.Rebuild;

public sealed class StatsProjectionRebuilder(
    ActivityDbContext db,
    IEnumerable<IStatsProjection> projections) : IStatsProjectionRebuilder
{
    private const int BatchSize = 1000;

    public async Task<RebuildResult> RebuildAsync(CancellationToken ct = default)
    {
        // Пересборка целиком в одной транзакции:
        // либо статистика полностью новая, либо остаётся старой.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var statsProjections = projections.ToArray();

        // 1. Очищаем только stats-таблицы.
        //    library_entries не трогаем: LibraryEntryProjection сюда не входит.
        foreach (var projection in statsProjections)
            await projection.ResetAsync(ct);

        long lastPosition = 0;
        var eventsProcessed = 0;
        var batches = 0;

        // 2. Читаем события порциями по 1000, а не всю таблицу сразу.
        while (true)
        {
            var records = await db.Events
                .AsNoTracking()
                .Where(e => e.Position > lastPosition)
                .OrderBy(e => e.Position)
                .Take(BatchSize)
                .ToListAsync(ct);

            if (records.Count == 0)
                break;

            batches++;

            foreach (var record in records)
            {
                // StreamId = "reading-{userId}-{mangaId}"
                var (userId, mangaId) = ReadingStream.Parse(record.StreamId);

                var readingEvent = EventSerializer.Deserialize(
                    record.Type, record.Data);

                var projectionEvent = new ProjectionEvent(
                    readingEvent,
                    userId,
                    mangaId,
                    record.Version,
                    record.Position);

                foreach (var projection in statsProjections)
                    await projection.ApplyAsync(projectionEvent, ct);
            }

            eventsProcessed += records.Count;
            lastPosition = records[^1].Position;
        }

        await transaction.CommitAsync(ct);

        return new RebuildResult(eventsProcessed, batches);
    }
}
