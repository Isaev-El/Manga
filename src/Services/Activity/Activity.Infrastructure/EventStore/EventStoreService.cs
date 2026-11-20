using Activity.Application;
using Activity.Domain.Common;
using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Activity.Infrastructure.EventStore;

/// <summary>Сохранённое событие вместе с номером версии в своём потоке.</summary>
public sealed record StoredEvent(int Version, DomainEvent Event);

/// <summary>
/// Общая работа с таблицей events: чтение потока и добавление новых событий.
/// Используется всеми агрегатами сервиса (сейчас — ReadingEntry, дальше — ChapterComment).
///
/// Класс назван EventStoreService, а не EventStore, потому что в этом namespace
/// уже есть одноимённый namespace Activity.Infrastructure.EventStore: C# не позволил бы
/// обращаться к классу EventStore по короткому имени.
/// </summary>
public sealed class EventStoreService(ActivityDbContext db)
{
    /// <summary>Читает все события потока по порядку версий.</summary>
    public async Task<IReadOnlyList<DomainEvent>> ReadStreamAsync(
        string streamId, CancellationToken ct = default) =>
        (await ReadStreamWithVersionsAsync(streamId, ct)).Select(x => x.Event).ToList();

    /// <summary>То же, но с номерами версий — нужно для истории событий.</summary>
    public async Task<IReadOnlyList<StoredEvent>> ReadStreamWithVersionsAsync(
        string streamId, CancellationToken ct = default)
    {
        var records = await db.Events
            .AsNoTracking()
            .Where(e => e.StreamId == streamId)
            .OrderBy(e => e.Version)
            .ToListAsync(ct);

        return records
            .Select(r => new StoredEvent(r.Version, EventSerializer.Deserialize(r.Type, r.Data)))
            .ToList();
    }

    /// <summary>
    /// Дописывает события в конец потока. expectedVersion — версия, которую загрузил агрегат.
    /// SaveChanges вызывается здесь и сохраняет всё накопленное в DbContext (события и проекции)
    /// одной транзакцией. Если параллельный запрос успел записать ту же версию, уникальный
    /// индекс (StreamId, Version) вызовет ошибку 23505 -> 409 Conflict.
    /// </summary>
    public async Task AppendAsync(
        string streamId,
        int expectedVersion,
        IEnumerable<DomainEvent> events,
        CancellationToken ct = default)
    {
        var newEvents = events.ToList();
        if (newEvents.Count == 0)
            return;

        // Новые события получают номера сразу после последнего загруженного
        var version = expectedVersion;
        foreach (var e in newEvents)
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
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
                  { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Кто-то успел записать событие с тем же номером раньше нас
            db.ChangeTracker.Clear();
            throw new ConflictException(
                "Данные изменились одновременно с вашим запросом. Повторите действие");
        }
    }
}
