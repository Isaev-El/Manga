using Activity.Application.Abstractions;
using Activity.Domain.Events;
using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.Projections.Stats;

public sealed class DailyReadingStatsProjection(ActivityDbContext db)
    : IStatsProjection
{
    private static readonly TimeSpan KazakhstanOffset = TimeSpan.FromHours(5);

    public async Task ApplyAsync(ProjectionEvent e, CancellationToken ct = default)
    {
        var day = DateOnly.FromDateTime(
            e.Event.OccurredOnUtc.Add(KazakhstanOffset));

        switch (e.Event)
        {
            case ChapterRead:
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                               INSERT INTO "daily_reading_stats"
                                                                   ("Day", "ChaptersRead", "ActiveReaders", "TitlesAdded")
                                                               VALUES ({day}, 1, 0, 0)
                                                               ON CONFLICT ("Day") DO UPDATE
                                                               SET "ChaptersRead" = "daily_reading_stats"."ChaptersRead" + 1;
                                                               """, ct);

                var inserted = await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                                              INSERT INTO "daily_active_readers" ("Day", "UserId")
                                                                              VALUES ({day}, {e.UserId})
                                                                              ON CONFLICT ("Day", "UserId") DO NOTHING;
                                                                              """, ct);

                if (inserted > 0)
                {
                    await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                                   UPDATE "daily_reading_stats"
                                                                   SET "ActiveReaders" = "ActiveReaders" + 1
                                                                   WHERE "Day" = {day};
                                                                   """, ct);
                }
                break;

            case AddedToList:
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                               INSERT INTO "daily_reading_stats"
                                                                   ("Day", "ChaptersRead", "ActiveReaders", "TitlesAdded")
                                                               VALUES ({day}, 0, 0, 1)
                                                               ON CONFLICT ("Day") DO UPDATE
                                                               SET "TitlesAdded" = "daily_reading_stats"."TitlesAdded" + 1;
                                                               """, ct);
                break;
        }
    }

    public async Task ResetAsync(CancellationToken ct = default)
    {
        await db.DailyActiveReaderRows.ExecuteDeleteAsync(ct);
        await db.DailyReadingStatsRows.ExecuteDeleteAsync(ct);
    }
}
