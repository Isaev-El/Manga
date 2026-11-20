namespace Activity.Application.Abstractions;

/// <summary>
/// Сторона чтения комментариев: список из проекции chapter_comments
/// и история из таблицы events.
/// </summary>
public interface ICommentReadModel
{
    Task<CommentPageDto> GetCommentsAsync(
        Guid chapterId, int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<CommentHistoryItemDto>> GetHistoryAsync(
        Guid commentId, CancellationToken ct = default);
}
