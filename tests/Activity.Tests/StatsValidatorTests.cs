using Activity.Application.Features.Stats;

namespace Activity.Tests;

public class StatsValidatorTests
{
    [Fact]
    public async Task Reading_stats_validator_rejects_reversed_period()
    {
        var validator = new GetReadingStatsQueryValidator();

        var result = await validator.ValidateAsync(
            new GetReadingStatsQuery(
                new DateOnly(2026, 1, 2),
                new DateOnly(2026, 1, 1)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Reading_stats_validator_rejects_period_over_90_days()
    {
        var validator = new GetReadingStatsQueryValidator();

        var result = await validator.ValidateAsync(
            new GetReadingStatsQuery(
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 4, 1)));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Top_manga_validator_rejects_bad_limit(int limit)
    {
        var validator = new GetTopMangaQueryValidator();

        var result = await validator.ValidateAsync(new GetTopMangaQuery(limit));

        Assert.False(result.IsValid);
    }
}
