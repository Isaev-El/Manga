using Activity.Application.Features.Ratings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Activity.Api.Controllers;

[ApiController]
public class RatingsController(ISender sender) : ControllerBase
{
    /// <summary>Средняя оценка тайтла по всем пользователям.</summary>
    [HttpGet("api/manga/{mangaId:guid}/rating")]
    public async Task<IActionResult> Get(Guid mangaId, CancellationToken ct) =>
        Ok(await sender.Send(new GetMangaRatingQuery(mangaId), ct));
}
