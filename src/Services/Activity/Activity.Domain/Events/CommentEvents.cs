using Activity.Domain.Common;

namespace Activity.Domain.Events;

/// <summary>
/// Событие из истории комментария (поток comment-{id}).
/// События — это факты: они только добавляются и никогда не меняются и не удаляются.
/// Из них вычисляется состояние ChapterComment и строятся проекции.
/// </summary>
public abstract record CommentEvent : DomainEvent;

/// <summary>Пользователь оставил комментарий к главе.</summary>
public sealed record CommentPosted(
    Guid CommentId,
    Guid ChapterId,
    Guid MangaId,
    string UserId,
    string Text,
    Guid? ParentCommentId = null) : CommentEvent;

/// <summary>Автор изменил текст комментария.</summary>
public sealed record CommentEdited(string Text) : CommentEvent;

/// <summary>Автор удалил комментарий.</summary>
public sealed record CommentDeleted : CommentEvent;
