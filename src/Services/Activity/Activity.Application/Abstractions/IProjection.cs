using Activity.Domain.Events;

namespace Activity.Application.Abstractions;

public interface IProjection
{
    Task ApplyAsync(ProjectionEvent e, CancellationToken ct = default);
    Task ResetAsync(CancellationToken ct = default);
}

public interface IStatsProjection : IProjection { }

public sealed record ProjectionEvent(
    ReadingEvent Event,
    string UserId,
    Guid MangaId,
    int Version,
    long? Position = null);
