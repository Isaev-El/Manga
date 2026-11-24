using Activity.Application.Abstractions;
using Activity.Domain;
using Activity.Domain.Common;
using Activity.Domain.Events;
using Activity.Infrastructure.EventStore;
using Activity.Infrastructure.Persistence;
using Activity.Infrastructure.Projections;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.Repositories;

/// <summary>
/// Репозиторий агрегата поверх хранилища событий.
/// Load — читает поток и проигрывает его. Save — дописывает новые события
/// и обновляет проекцию одной транзакцией.
/// </summary>
public class ReadingEntryRepository(ActivityDbContext db, EventStoreService eventStore)
    : IReadingEntryRepository
{
    public async Task<ReadingEntry> LoadAsync(string userId, Guid mangaId, CancellationToken ct = default)
    {
        var streamId = ReadingEntry.StreamId(userId, mangaId);

        var history = await eventStore.ReadStreamAsync(streamId, ct);
        return ReadingEntry.Load(userId, mangaId, history.Select(ToReadingEvent));
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

        // Проекция получает те же номера версий, что и новые события
        var version = entry.Version;
        foreach (var e in entry.UncommittedEvents)
        {
            version++;
            LibraryEntryProjector.Apply(row, e, version);
        }

        // События и проекция сохраняются одной транзакцией.
        // При гонке уникальный индекс (StreamId, Version) даст 409.
        await eventStore.AppendAsync(streamId, entry.Version, entry.UncommittedEvents, ct);

        entry.MarkEventsAsCommitted();
    }

    private static ReadingEvent ToReadingEvent(DomainEvent e) =>
        e as ReadingEvent
        ?? throw new InvalidOperationException(
            $"Событие {e.GetType().Name} не принадлежит потоку чтения");
}
