using Catalog.Domain.Common;

namespace Catalog.Domain.Events;

public sealed record MangaCreated(Guid MangaId, string Title) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record MangaCompleted(Guid MangaId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
