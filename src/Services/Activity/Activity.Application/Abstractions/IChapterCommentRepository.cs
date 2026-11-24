using Activity.Domain;

namespace Activity.Application.Abstractions;

/// <summary>
/// Репозиторий агрегата ChapterComment. Снаружи выглядит как обычный репозиторий,
/// а внутри (в Infrastructure) читает и дописывает поток комментария.
/// </summary>
public interface IChapterCommentRepository
{
    /// <summary>
    /// Загружает историю комментария. null — в потоке comment-{id} нет событий,
    /// то есть комментария не существует.
    /// </summary>
    Task<ChapterComment?> LoadAsync(Guid commentId, CancellationToken ct = default);

    /// <summary>
    /// Дописывает новые события агрегата и обновляет проекцию chapter_comments
    /// одной транзакцией. При гонке бросает ConflictException.
    /// </summary>
    Task SaveAsync(ChapterComment comment, CancellationToken ct = default);
}
