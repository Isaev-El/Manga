using System.Text.Json;

namespace Activity.Application;

/// <summary>Строка из списков пользователя (из проекции library_entries).</summary>
public record LibraryEntryDto(
    Guid MangaId,
    string Status,
    Guid? LastReadChapterId,
    decimal LastReadChapterNumber,
    int? Rating,
    DateTime UpdatedAtUtc);

/// <summary>Одно событие из истории — прямо из таблицы events.</summary>
public record HistoryItemDto(int Version, string Event, JsonElement Data, DateTime OccurredOnUtc);

/// <summary>Средняя оценка тайтла по всем пользователям.</summary>
public record MangaRatingDto(Guid MangaId, double? Average, int Votes);
