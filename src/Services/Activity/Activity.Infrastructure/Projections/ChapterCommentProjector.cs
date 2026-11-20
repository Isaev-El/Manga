using Activity.Domain.Events;

namespace Activity.Infrastructure.Projections;

/// <summary>
/// Логика проекции chapter_comments: как каждое событие меняет строку таблицы.
/// Агрегат собирает состояние для проверки правил, а проектор — таблицу для чтения.
/// </summary>
public static class ChapterCommentProjector
{
    public static void Apply(ChapterCommentRow row, CommentEvent e, int version)
    {
        switch (e)
        {
            case CommentPosted posted:
                row.CommentId = posted.CommentId;
                row.ChapterId = posted.ChapterId;
                row.UserId = posted.UserId;
                row.Text = posted.Text;
                row.ParentCommentId = posted.ParentCommentId;
                row.IsDeleted = false;
                row.EditCount = 0;
                row.CreatedAtUtc = posted.OccurredOnUtc;
                row.UpdatedAtUtc = posted.OccurredOnUtc;
                break;

            case CommentEdited edited:
                row.Text = edited.Text;
                row.EditCount++;
                row.UpdatedAtUtc = edited.OccurredOnUtc;
                break;

            case CommentDeleted deleted:
                row.IsDeleted = true;
                row.UpdatedAtUtc = deleted.OccurredOnUtc;
                break;
        }

        row.Version = version;
    }
}
