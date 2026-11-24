using Activity.Application.Features.Comments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Activity.Api.Controllers;

public record PostCommentRequest(string Text, Guid? ParentCommentId = null);
public record EditCommentRequest(string Text);

/// <summary>Команды комментариев. Чтение списка комментариев — отдельная read-model.</summary>
[ApiController]
public class CommentsController(ISender sender) : ControllerBase
{
    /// <summary>Оставить комментарий к опубликованной главе.</summary>
    [HttpPost("api/chapters/{chapterId:guid}/comments")]
    public async Task<IActionResult> Post(
        [FromHeader(Name = "X-User-Id")] string userId,
        Guid chapterId,
        PostCommentRequest request,
        CancellationToken ct)
    {
        var id = await sender.Send(
            new PostCommentCommand(userId, chapterId, request.Text, request.ParentCommentId), ct);

        return StatusCode(StatusCodes.Status201Created, new { id });
    }

    /// <summary>Изменить свой комментарий. Тот же текст — 204 без записи.</summary>
    [HttpPut("api/comments/{commentId:guid}")]
    public async Task<IActionResult> Edit(
        [FromHeader(Name = "X-User-Id")] string userId,
        Guid commentId,
        EditCommentRequest request,
        CancellationToken ct)
    {
        await sender.Send(new EditCommentCommand(userId, commentId, request.Text), ct);
        return NoContent();
    }

    /// <summary>Удалить свой комментарий. Повторное удаление — 204 без записи.</summary>
    [HttpDelete("api/comments/{commentId:guid}")]
    public async Task<IActionResult> Delete(
        [FromHeader(Name = "X-User-Id")] string userId,
        Guid commentId,
        CancellationToken ct)
    {
        await sender.Send(new DeleteCommentCommand(userId, commentId), ct);
        return NoContent();
    }

    /// <summary>Комментарии главы по страницам. Удалённые — с подменённым текстом.</summary>
    [HttpGet("api/chapters/{chapterId:guid}/comments")]
    public async Task<IActionResult> List(
        Guid chapterId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        Ok(await sender.Send(new ListCommentsQuery(chapterId, page, pageSize), ct));

    /// <summary>История комментария: версии событий из таблицы events.</summary>
    [HttpGet("api/comments/{commentId:guid}/history")]
    public async Task<IActionResult> History(Guid commentId, CancellationToken ct) =>
        Ok(await sender.Send(new GetCommentHistoryQuery(commentId), ct));
}
