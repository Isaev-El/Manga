using Activity.Domain;

namespace Activity.Application.Abstractions;

/// <summary>
/// Репозиторий агрегата ReadingEntry. Снаружи выглядит как обычный репозиторий,
/// а внутри (в Infrastructure) читает и дописывает поток событий.
/// </summary>
public interface IReadingEntryRepository
{
    /// <summary>Загружает историю и восстанавливает состояние. Если истории нет — пустая запись.</summary>
    Task<ReadingEntry> LoadAsync(string userId, Guid mangaId, CancellationToken ct = default);

    /// <summary>
    /// Дописывает новые события агрегата и обновляет проекции — одной транзакцией.
    /// Если кто-то успел изменить поток раньше, бросает ConflictException.
    /// </summary>
    Task SaveAsync(ReadingEntry entry, CancellationToken ct = default);
}
