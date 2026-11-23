using Activity.Application.Features.Stats;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Activity.Api.Controllers;

[ApiController]
[Route("api/admin/stats")]
public class AdminStatsController(ISender sender) : ControllerBase
{
    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild(CancellationToken ct) =>
        Ok(await sender.Send(new RebuildStatsCommand(), ct));
}
