using Activity.Domain.Events;

namespace Activity.Infrastructure.Projections;

/// <summary>
/// Логика проекции: как каждое событие меняет строку library_entries.
///
/// Похоже на ReadingEntry.Apply, но это РАЗНЫЕ вещи:
/// агрегат собирает состояние, чтобы проверять правила при записи,
/// а проектор собирает таблицу под конкретный экран для чтения.
/// Для другого экрана будет другая проекция со своим проектором.
/// </summary>
public static class LibraryEntryProjector
{
    public static void Apply(LibraryEntryRow row, ReadingEvent e, int version)
    {
        switch (e)
        {
            case AddedToList added:
                row.Status = added.Status;
                break;

            case ListStatusChanged changed:
                row.Status = changed.To;
                break;

            case ChapterRead read when read.Number > row.LastReadChapterNumber:
                row.LastReadChapterNumber = read.Number;
                row.LastReadChapterId = read.ChapterId;
                break;

            case ChapterRead:
                break; // перечитал старую главу — прогресс не меняется

            case Rated rated:
                row.Rating = rated.Score;
                break;

            case RemovedFromList:
                row.Status = null;
                row.Rating = null;
                break;
        }

        row.Version = version;
        row.UpdatedAtUtc = e.OccurredOnUtc;
    }
}
