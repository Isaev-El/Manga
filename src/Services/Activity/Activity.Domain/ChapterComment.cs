using Activity.Domain.Common;
using Activity.Domain.Events;

namespace Activity.Domain;

/// <summary>
/// Агрегат "комментарий к главе". Как и ReadingEntry, хранится потоком событий,
/// а не строкой в таблице. Состояние собирается проигрыванием истории (Apply),
/// команды проверяют правила и создают новые события (Raise).
/// </summary>
public sealed class ChapterComment
{
    private readonly List<CommentEvent> _uncommittedEvents = new();

    public Guid Id { get; private set; }
    public Guid ChapterId { get; private set; }
    public Guid MangaId { get; private set; }
    public string UserId { get; private set; } = null!;
    public string Text { get; private set; } = null!;
    public Guid? ParentCommentId { get; private set; }
    public bool IsDeleted { get; private set; }

    /// <summary>Сколько событий потока уже сохранено (версия, с которой его загрузили).</summary>
    public int Version { get; private set; }

    /// <summary>Новые события, созданные командами и ещё не записанные в хранилище.</summary>
    public IReadOnlyList<CommentEvent> UncommittedEvents => _uncommittedEvents;

    private ChapterComment(Guid id) => Id = id;

    /// <summary>Идентификатор потока событий: одна история на комментарий.</summary>
    public static string StreamId(Guid commentId) => $"comment-{commentId}";

    /// <summary>Создаёт новый комментарий и поднимает CommentPosted.</summary>
    public static ChapterComment Post(
        Guid chapterId, Guid mangaId, string userId, string text, Guid? parentCommentId = null)
    {
        if (chapterId == Guid.Empty)
            throw new DomainException("Не указана глава");
        if (mangaId == Guid.Empty)
            throw new DomainException("Не указан тайтл");
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("Не указан пользователь");
        if (parentCommentId == Guid.Empty)
            throw new DomainException("Не указан родительский комментарий");

        var comment = new ChapterComment(Guid.NewGuid());
        comment.Raise(new CommentPosted(
            comment.Id, chapterId, mangaId, userId, NormalizeText(text), parentCommentId));
        return comment;
    }

    /// <summary>Восстанавливает состояние, проигрывая историю событий.</summary>
    public static ChapterComment Load(Guid commentId, IEnumerable<CommentEvent> history)
    {
        if (commentId == Guid.Empty)
            throw new DomainException("Не указан комментарий");

        var comment = new ChapterComment(commentId);
        foreach (var e in history)
        {
            comment.Apply(e);
            comment.Version++;
        }
        return comment;
    }

    /// <summary>
    /// Меняет текст. Возвращает false, если текст не изменился:
    /// тогда событие не создаётся и в БД ничего не пишется.
    /// </summary>
    public bool Edit(string userId, string newText)
    {
        EnsureOwner(userId);

        if (IsDeleted)
            throw new DomainException("Нельзя редактировать удалённый комментарий");

        var normalized = NormalizeText(newText);
        if (normalized == Text)
            return false;

        Raise(new CommentEdited(normalized));
        return true;
    }

    /// <summary>
    /// Удаляет комментарий. Возвращает false, если он уже удалён:
    /// повторное удаление ничего не пишет в БД.
    /// </summary>
    public bool Delete(string userId)
    {
        EnsureOwner(userId);

        if (IsDeleted)
            return false;

        Raise(new CommentDeleted());
        return true;
    }

    /// <summary>
    /// Как событие меняет состояние. Здесь нет проверок: событие — уже случившийся факт.
    /// Используется и при загрузке истории, и при создании нового события через Raise.
    /// </summary>
    public void Apply(DomainEvent e)
    {
        switch (e)
        {
            case CommentPosted posted:
                Id = posted.CommentId;
                ChapterId = posted.ChapterId;
                MangaId = posted.MangaId;
                UserId = posted.UserId;
                Text = posted.Text;
                ParentCommentId = posted.ParentCommentId;
                break;

            case CommentEdited edited:
                Text = edited.Text;
                break;

            case CommentDeleted:
                IsDeleted = true;
                break;

            default:
                throw new InvalidOperationException($"Неизвестное событие: {e.GetType().Name}");
        }
    }

    /// <summary>Вызывается репозиторием после успешного сохранения.</summary>
    public void MarkEventsAsCommitted()
    {
        Version += _uncommittedEvents.Count;
        _uncommittedEvents.Clear();
    }

    private void Raise(CommentEvent e)
    {
        Apply(e);
        _uncommittedEvents.Add(e);
    }

    private void EnsureOwner(string userId)
    {
        if (!string.Equals(userId, UserId, StringComparison.Ordinal))
            throw new ForbiddenException("Редактировать и удалять комментарий может только автор");
    }

    private static string NormalizeText(string text)
    {
        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length is < 1 or > 2000)
            throw new DomainException("Комментарий должен быть от 1 до 2000 символов");
        return trimmed;
    }
}
