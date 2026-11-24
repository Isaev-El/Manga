using Activity.Application.Features.Stats;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Activity.Api.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController(ISender sender) : ControllerBase
{
    [HttpGet("reading")]
    public async Task<IActionResult> GetReading(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetReadingStatsQuery(from, to), ct));

    [HttpGet("top-manga")]
    public async Task<IActionResult> GetTopManga(
        [FromQuery] int limit = 10,
        CancellationToken ct = default) =>
        Ok(await sender.Send(new GetTopMangaQuery(limit), ct));

    [HttpGet("drop-off/{mangaId:guid}")]
    public async Task<IActionResult> GetDropOff(
        Guid mangaId,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetDropOffQuery(mangaId), ct));
}
