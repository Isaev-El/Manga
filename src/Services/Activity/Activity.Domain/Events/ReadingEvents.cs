namespace Activity.Domain.Events;

/// <summary>
/// Событие из истории "пользователь + тайтл".
/// События — это факты: они только добавляются и никогда не меняются и не удаляются.
/// Из них вычисляется текущее состояние (ReadingEntry) и строятся проекции.
/// </summary>
public abstract record ReadingEvent
{
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}

/// <summary>Добавил тайтл в список.</summary>
public sealed record AddedToList(ListStatus Status) : ReadingEvent;

/// <summary>Переложил тайтл в другой список.</summary>
public sealed record ListStatusChanged(ListStatus From, ListStatus To) : ReadingEvent;

/// <summary>Прочитал главу.</summary>
public sealed record ChapterRead(Guid ChapterId, decimal Number) : ReadingEvent;

/// <summary>Поставил или изменил оценку.</summary>
public sealed record Rated(int Score) : ReadingEvent;

/// <summary>Убрал тайтл из списков.</summary>
public sealed record RemovedFromList : ReadingEvent;
