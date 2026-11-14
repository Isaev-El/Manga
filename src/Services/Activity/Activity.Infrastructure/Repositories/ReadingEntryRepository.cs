using Activity.Application;
using Activity.Application.Abstractions;
using Activity.Domain;
using Activity.Infrastructure.EventStore;
using Activity.Infrastructure.Persistence;
using Activity.Infrastructure.Projections;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Activity.Infrastructure.Repositories;

/// <summary>
/// Репозиторий агрегата поверх хранилища событий.
/// Load — читает поток и проигрывает его. Save — дописывает новые события
/// и обновляет проекцию одной транзакцией.
/// </summary>
public class ReadingEntryRepository(ActivityDbContext db) : IReadingEntryRepository
{
    public async Task<ReadingEntry> LoadAsync(string userId, Guid mangaId, CancellationToken ct = default)
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
            return; // ничего не изменилось

        var streamId = ReadingEntry.StreamId(entry.UserId, entry.MangaId);

        // Строка проекции: есть — обновим, нет — создадим
        var row = await db.LibraryEntries
            .FirstOrDefaultAsync(x => x.UserId == entry.UserId && x.MangaId == entry.MangaId, ct);

        if (row is null)
        {
            row = new LibraryEntryRow { UserId = entry.UserId, MangaId = entry.MangaId };
            db.LibraryEntries.Add(row);
        }

        // Новые события получают номера сразу после последнего загруженного
        var version = entry.Version;
        foreach (var e in entry.UncommittedEvents)
        {
            version++;

            // 1. Дописываем событие в конец истории
            db.Events.Add(new EventRecord
            {
                StreamId = streamId,
                Version = version,
                Type = EventSerializer.TypeName(e),
                Data = EventSerializer.Serialize(e),
                OccurredOnUtc = e.OccurredOnUtc
            });

            // 2. Обновляем проекцию тем же событием
            LibraryEntryProjector.Apply(row, e, version);
        }

        try
        {
            // 3. Одна транзакция: и события, и проекция — либо всё, либо ничего
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Кто-то успел записать событие с тем же номером раньше нас
            db.ChangeTracker.Clear();
            throw new ConflictException("Данные изменились одновременно с вашим запросом. Повторите действие");
        }

        entry.MarkEventsAsCommitted();
    }
}
