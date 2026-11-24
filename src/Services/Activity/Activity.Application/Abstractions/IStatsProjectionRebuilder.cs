namespace Activity.Application.Abstractions;

public interface IStatsProjectionRebuilder
{
    Task<RebuildResult> RebuildAsync(CancellationToken ct = default);
}
