using Activity.Application;
using Activity.Application.Abstractions;
using Activity.Domain;
using Activity.Domain.Events;
using Activity.Infrastructure.EventStore;
using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.ReadModel;

/// <summary>Чтение комментариев: проекция для списка, events — для истории версий.</summary>
public class CommentReadModel(ActivityDbContext db, EventStoreService eventStore) : ICommentReadModel
{
    private const string DeletedText = "Комментарий удалён";

    public async Task<CommentPageDto> GetCommentsAsync(
        Guid chapterId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.ChapterComments
            .AsNoTracking()
            .Where(x => x.ChapterId == chapterId);

        var total = await query.CountAsync(ct);

        // long, чтобы огромный page не переполнил int при умножении
        var skip = (long)(page - 1) * pageSize;

        var items = skip > int.MaxValue
            ? new List<CommentDto>()
            : await query
                .OrderBy(x => x.CreatedAtUtc)
                .ThenBy(x => x.CommentId)
                .Skip((int)skip)
                .Take(pageSize)
                .Select(x => new CommentDto(
                    x.CommentId,
                    x.ChapterId,
                    x.ParentCommentId,
                    x.UserId,
                    x.IsDeleted ? DeletedText : x.Text,
                    x.IsDeleted,
                    x.EditCount,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc))
                .ToListAsync(ct);

        return new CommentPageDto(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<CommentHistoryItemDto>> GetHistoryAsync(
        Guid commentId, CancellationToken ct = default)
    {
        var events = await eventStore.ReadStreamWithVersionsAsync(
            ChapterComment.StreamId(commentId), ct);

        return events
            .Select(x => new CommentHistoryItemDto(
                x.Version,
                x.Event.GetType().Name,
                x.Event switch
                {
                    CommentPosted posted => posted.Text,
                    CommentEdited edited => edited.Text,
                    _ => null // у CommentDeleted текста нет
                },
                x.Event.OccurredOnUtc))
            .ToList();
    }
}
