namespace Activity.Infrastructure.Projections;

/// <summary>
/// Строка проекции chapter_comments: готовый итог событий потока comment-{id}.
/// Её можно удалить и собрать заново из таблицы events — ничего не потеряется.
/// </summary>
public class ChapterCommentRow
{
    public Guid CommentId { get; set; }
    public Guid ChapterId { get; set; }
    public string UserId { get; set; } = null!;
    public string Text { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public int EditCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>Версия последнего применённого события потока.</summary>
    public int Version { get; set; }
}
