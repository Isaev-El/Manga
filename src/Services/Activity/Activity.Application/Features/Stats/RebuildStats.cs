using Activity.Application.Abstractions;
using MediatR;

namespace Activity.Application.Features.Stats;

public record RebuildStatsCommand : IRequest<RebuildResult>;

public class RebuildStatsCommandHandler(IStatsProjectionRebuilder rebuilder)
    : IRequestHandler<RebuildStatsCommand, RebuildResult>
{
    public Task<RebuildResult> Handle(
        RebuildStatsCommand command, CancellationToken ct) =>
        rebuilder.RebuildAsync(ct);
}
