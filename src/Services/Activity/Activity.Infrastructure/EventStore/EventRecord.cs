namespace Activity.Infrastructure.EventStore;

/// <summary>
/// Строка таблицы events — ГЛАВНАЯ таблица сервиса, источник правды.
/// Записи только добавляются: не меняются и не удаляются.
/// </summary>
public class EventRecord
{
    /// <summary>Сквозной номер события во всём сервисе (порядок всех событий).</summary>
    public long Position { get; set; }

    /// <summary>Чья это история: "reading-{userId}-{mangaId}".</summary>
    public string StreamId { get; set; } = null!;

    /// <summary>Номер события внутри своей истории: 1, 2, 3...</summary>
    public int Version { get; set; }

    /// <summary>Тип события: "AddedToList", "ChapterRead", ...</summary>
    public string Type { get; set; } = null!;

    /// <summary>Данные события в JSON.</summary>
    public string Data { get; set; } = null!;

    public DateTime OccurredOnUtc { get; set; }
}
