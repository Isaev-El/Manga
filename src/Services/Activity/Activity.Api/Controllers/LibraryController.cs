using Activity.Application.Features.Library;
using Activity.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Activity.Api.Controllers;

public record AddToListRequest(Guid MangaId, ListStatus Status);
public record ChangeStatusRequest(ListStatus Status);
public record MarkChapterReadRequest(Guid ChapterId);
public record RateRequest(int Score);

/// <summary>
/// Списки и прогресс пользователя. Пользователь пока передаётся заголовком X-User-Id —
/// когда появится сервис Identity, он будет браться из JWT-токена.
/// </summary>
[ApiController]
[Route("api/library")]
public class LibraryController(ISender sender) : ControllerBase
{
    /// <summary>Мои списки (можно отфильтровать: ?status=Reading).</summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromHeader(Name = "X-User-Id")] string userId, [FromQuery] ListStatus? status, CancellationToken ct) =>
        Ok(await sender.Send(new GetLibraryQuery(userId, status), ct));

    /// <summary>Добавить тайтл в список.</summary>
    [HttpPost]
    public async Task<IActionResult> Add(
        [FromHeader(Name = "X-User-Id")] string userId, AddToListRequest request, CancellationToken ct)
    {
        await sender.Send(new AddToListCommand(userId, request.MangaId, request.Status), ct);
        return NoContent();
    }

    /// <summary>Переложить тайтл в другой список.</summary>
    [HttpPut("{mangaId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        [FromHeader(Name = "X-User-Id")] string userId, Guid mangaId, ChangeStatusRequest request, CancellationToken ct)
    {
        await sender.Send(new ChangeStatusCommand(userId, mangaId, request.Status), ct);
        return NoContent();
    }

    /// <summary>Отметить главу прочитанной (если тайтла нет в списках — добавится в "Читаю").</summary>
    [HttpPost("{mangaId:guid}/read")]
    public async Task<IActionResult> MarkRead(
        [FromHeader(Name = "X-User-Id")] string userId, Guid mangaId, MarkChapterReadRequest request, CancellationToken ct)
    {
        await sender.Send(new MarkChapterReadCommand(userId, mangaId, request.ChapterId), ct);
        return NoContent();
    }

    /// <summary>Поставить или изменить оценку (1–10).</summary>
    [HttpPut("{mangaId:guid}/rating")]
    public async Task<IActionResult> Rate(
        [FromHeader(Name = "X-User-Id")] string userId, Guid mangaId, RateRequest request, CancellationToken ct)
    {
        await sender.Send(new RateMangaCommand(userId, mangaId, request.Score), ct);
        return NoContent();
    }

    /// <summary>Убрать тайтл из списков.</summary>
    [HttpDelete("{mangaId:guid}")]
    public async Task<IActionResult> Remove(
        [FromHeader(Name = "X-User-Id")] string userId, Guid mangaId, CancellationToken ct)
    {
        await sender.Send(new RemoveFromListCommand(userId, mangaId), ct);
        return NoContent();
    }

    /// <summary>Вся история действий с тайтлом — события как есть.</summary>
    [HttpGet("{mangaId:guid}/history")]
    public async Task<IActionResult> History(
        [FromHeader(Name = "X-User-Id")] string userId, Guid mangaId, CancellationToken ct) =>
        Ok(await sender.Send(new GetHistoryQuery(userId, mangaId), ct));
}
