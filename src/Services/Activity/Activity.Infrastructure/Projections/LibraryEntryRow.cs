using Activity.Domain;

namespace Activity.Infrastructure.Projections;

/// <summary>
/// Строка проекции library_entries — готовый ИТОГ всех событий пары "пользователь + тайтл".
/// Нужна только для быстрого чтения ("мои списки", средняя оценка).
/// Её можно удалить и собрать заново из таблицы events — ничего не потеряется.
/// </summary>
public class LibraryEntryRow
{
    public string UserId { get; set; } = null!;
    public Guid MangaId { get; set; }

    /// <summary>null — тайтл убран из списков.</summary>
    public ListStatus? Status { get; set; }

    public Guid? LastReadChapterId { get; set; }
    public decimal LastReadChapterNumber { get; set; }
    public int? Rating { get; set; }

    /// <summary>Версия последнего учтённого события потока.</summary>
    public int Version { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
