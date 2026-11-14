using Catalog.Domain.Common;

namespace Catalog.Domain.Events;

/// <summary>
/// Глава опубликована. Именно это событие позже будет слушать сервис Activity,
/// чтобы уведомить читателей о новой главе.
/// </summary>
public sealed record ChapterPublished(Guid ChapterId, Guid MangaId, decimal Number, string? Title) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
