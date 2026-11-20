using Activity.Application.Abstractions;
using Activity.Domain;
using Activity.Domain.Common;
using Activity.Domain.Events;
using Activity.Infrastructure.EventStore;
using Activity.Infrastructure.Persistence;
using Activity.Infrastructure.Projections;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.Repositories;

/// <summary>
/// Репозиторий комментария поверх хранилища событий.
/// Load — читает поток comment-{id}. Save — дописывает события и обновляет
/// проекцию chapter_comments одной транзакцией.
/// </summary>
public class ChapterCommentRepository(ActivityDbContext db, EventStoreService eventStore)
    : IChapterCommentRepository
{
    public async Task<ChapterComment?> LoadAsync(Guid commentId, CancellationToken ct = default)
    {
        var history = await eventStore.ReadStreamAsync(ChapterComment.StreamId(commentId), ct);
        if (history.Count == 0)
            return null; // комментария нет: поток пуст

        return ChapterComment.Load(commentId, history.Select(ToCommentEvent));
    }

    public async Task SaveAsync(ChapterComment comment, CancellationToken ct = default)
    {
        if (comment.UncommittedEvents.Count == 0)
            return;

        var streamId = ChapterComment.StreamId(comment.Id);

        var row = await db.ChapterComments
            .FirstOrDefaultAsync(x => x.CommentId == comment.Id, ct);

        if (row is null)
        {
            row = new ChapterCommentRow { CommentId = comment.Id };
            db.ChapterComments.Add(row);

            // Строки проекции нет (например, её удалили) — собираем её из всей истории,
            // иначе Edit/Delete оставили бы в таблице только новые изменения.
            var history = await eventStore.ReadStreamAsync(streamId, ct);
            var historyVersion = 0;
            foreach (var e in history)
            {
                historyVersion++;
                ChapterCommentProjector.Apply(row, ToCommentEvent(e), historyVersion);
            }
        }

        // Проекция получает те же номера версий, что и новые события
        var version = comment.Version;
        foreach (var e in comment.UncommittedEvents)
        {
            version++;
            ChapterCommentProjector.Apply(row, e, version);
        }

        // AppendAsync вызывает SaveChanges: строка проекции и события
        // сохраняются одной транзакцией. При гонке — 409.
        await eventStore.AppendAsync(streamId, comment.Version, comment.UncommittedEvents, ct);

        comment.MarkEventsAsCommitted();
    }

    private static CommentEvent ToCommentEvent(DomainEvent e) =>
        e as CommentEvent
        ?? throw new InvalidOperationException(
            $"Событие {e.GetType().Name} не принадлежит потоку комментария");
}
