using Catalog.Application.Features.Mangas;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

public record CreateMangaRequest(string Title, string Author, List<Guid>? GenreIds);

// Контроллер только переводит HTTP в команды и запросы и отправляет их через MediatR.
// Кто их обработает, контроллер не знает.
[ApiController]
[Route("api/manga")]
public class MangaController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? genreId, CancellationToken ct) =>
        Ok(await sender.Send(new ListMangaQuery(genreId), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetMangaQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateMangaRequest request, CancellationToken ct)
    {
        var id = await sender.Send(
            new CreateMangaCommand(request.Title, request.Author, request.GenreIds ?? []), ct);

        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        await sender.Send(new CompleteMangaCommand(id), ct);
        return NoContent();
    }
}
