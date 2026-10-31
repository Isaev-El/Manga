using Catalog.Application;
using Catalog.Application.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

public record CreateMangaRequest(string Title, string Author, List<Guid> GenreIds);

[ApiController]
[Route("api/manga")]
public class MangaController(MangaService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? genreId, CancellationToken ct) =>
        Ok(await service.ListAsync(genreId, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        Ok(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateMangaRequest request, CancellationToken ct)
    {
        var manga = new CreateMangaCommand(Title: request.Title, Author: request.Author, GenreIds: request.GenreIds);
        var id = await service.CreateAsync(manga, request.GenreIds ?? [], ct);

        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        await service.CompleteAsync(id, ct);
        return NoContent();
    }
}
