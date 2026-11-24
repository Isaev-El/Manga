using Activity.Application;
using Activity.Application.Abstractions;
using Activity.Domain;
using Activity.Infrastructure.EventStore;
using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Activity.Infrastructure.Repositories;

public class ReadingEntryRepository(
    ActivityDbContext db,
    IEnumerable<IProjection> projections) : IReadingEntryRepository
{
    public async Task<ReadingEntry> LoadAsync(
        string userId, Guid mangaId, CancellationToken ct = default)
    {
        var streamId = ReadingEntry.StreamId(userId, mangaId);

        var records = await db.Events
            .AsNoTracking()
            .Where(e => e.StreamId == streamId)
            .OrderBy(e => e.Version)
            .ToListAsync(ct);

        var history = records.Select(r => EventSerializer.Deserialize(r.Type, r.Data));
        return ReadingEntry.Load(userId, mangaId, history);
    }

    public async Task SaveAsync(ReadingEntry entry, CancellationToken ct = default)
    {
        if (entry.UncommittedEvents.Count == 0)
            return;

        var streamId = ReadingEntry.StreamId(entry.UserId, entry.MangaId);

        // Одна транзакция: события и все проекции сохраняются вместе.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var version = entry.Version;
            var projected = new List<ProjectionEvent>();

            foreach (var e in entry.UncommittedEvents)
            {
                version++;

                db.Events.Add(new EventRecord
                {
                    StreamId = streamId,
                    Version = version,
                    Type = EventSerializer.TypeName(e),
                    Data = EventSerializer.Serialize(e),
                    OccurredOnUtc = e.OccurredOnUtc
                });

                projected.Add(new ProjectionEvent(
                    e, entry.UserId, entry.MangaId, version));
            }

            // 1. Сохраняем события. После этого они уже в БД внутри транзакции.
            await db.SaveChangesAsync(ct);

            // 2. Обновляем все проекции.
            //    Репозиторий не знает, какие они: DI подставит список
            //    всех зарегистрированных IProjection.
            foreach (var projectionEvent in projected)
            {
                foreach (var projection in projections)
                    await projection.ApplyAsync(projectionEvent, ct);
            }

            // 3. EF сохраняет изменения LibraryEntryProjection.
            //    Stats-проекции пишут SQL сразу, им SaveChanges не нужен.
            await db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
                  { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await transaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();

            throw new ConflictException(
                "Данные изменились одновременно с вашим запросом. Повторите действие");
        }

        entry.MarkEventsAsCommitted();
    }
}
