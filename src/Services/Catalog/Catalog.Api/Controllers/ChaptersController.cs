using Catalog.Application;
using Catalog.Application.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

public record CreateChapterRequest(decimal Number, string? Title);
public record AddPageRequest(string ImageKey);

[ApiController]
public class ChaptersController(ChapterService service) : ControllerBase
{
    /// <summary>Главы тайтла по порядку номеров.</summary>
    [HttpGet("api/manga/{mangaId:guid}/chapters")]
    public async Task<IActionResult> List(Guid mangaId, CancellationToken ct) =>
        Ok(await service.ListAsync(mangaId, ct));

    /// <summary>Создать черновик главы.</summary>
    [HttpPost("api/manga/{mangaId:guid}/chapters")]
    public async Task<IActionResult> Create(Guid mangaId, CreateChapterRequest request, CancellationToken ct)
    {
        var id = await service.CreateAsync(
            new CreateChapterCommand(mangaId, request.Number, request.Title), ct);

        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Глава со страницами.</summary>
    [HttpGet("api/chapters/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        Ok(await service.GetAsync(id, ct));

    /// <summary>Добавить страницу в конец черновика.</summary>
    [HttpPost("api/chapters/{id:guid}/pages")]
    public async Task<IActionResult> AddPage(Guid id, AddPageRequest request, CancellationToken ct)
    {
        var number = await service.AddPageAsync(id, new AddPageCommand(request.ImageKey), ct);
        return Ok(new { number });
    }

    /// <summary>Удалить страницу из черновика, следующие сдвинутся.</summary>
    [HttpDelete("api/chapters/{id:guid}/pages/{number:int}")]
    public async Task<IActionResult> RemovePage(Guid id, int number, CancellationToken ct)
    {
        await service.RemovePageAsync(id, number, ct);
        return NoContent();
    }

    /// <summary>Опубликовать главу (нужна хотя бы одна страница).</summary>
    [HttpPost("api/chapters/{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        await service.PublishAsync(id, ct);
        return NoContent();
    }
}
