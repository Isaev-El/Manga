using Activity.Application.Abstractions;
using Activity.Domain;
using Activity.Domain.Events;
using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.Projections.Stats;

public sealed class MangaReadingStatsProjection(ActivityDbContext db)
    : IStatsProjection
{
    public async Task ApplyAsync(ProjectionEvent e, CancellationToken ct = default)
    {
        switch (e.Event)
        {
            case AddedToList added:
                await IncrementInLists(e.MangaId, ct);

                if (added.Status == ListStatus.Completed)
                    await MarkCompleted(e, ct);
                break;

            case ListStatusChanged changed:
                if (changed.To == ListStatus.Completed &&
                    changed.From != ListStatus.Completed)
                {
                    await MarkCompleted(e, ct);
                }
                else if (changed.From == ListStatus.Completed &&
                         changed.To != ListStatus.Completed)
                {
                    await UnmarkCompleted(e, ct);
                }

                break;

            case ChapterRead:
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                               INSERT INTO "manga_reading_stats"
                                                                   ("MangaId", "ChaptersRead", "InLists", "Completed")
                                                               VALUES ({e.MangaId}, 1, 0, 0)
                                                               ON CONFLICT ("MangaId") DO UPDATE
                                                               SET "ChaptersRead" = "manga_reading_stats"."ChaptersRead" + 1;
                                                               """, ct);
                break;

            case RemovedFromList:
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                               UPDATE "manga_reading_stats"
                                                               SET "InLists" =
                                                                   CASE WHEN "InLists" > 0 THEN "InLists" - 1 ELSE 0 END
                                                               WHERE "MangaId" = {e.MangaId};
                                                               """, ct);

                await UnmarkCompleted(e, ct);
                break;
        }
    }

    private async Task IncrementInLists(Guid mangaId, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                       INSERT INTO "manga_reading_stats"
                                                           ("MangaId", "ChaptersRead", "InLists", "Completed")
                                                       VALUES ({mangaId}, 0, 1, 0)
                                                       ON CONFLICT ("MangaId") DO UPDATE
                                                       SET "InLists" = "manga_reading_stats"."InLists" + 1;
                                                       """, ct);
    }

    private async Task MarkCompleted(ProjectionEvent e, CancellationToken ct)
    {
        var inserted = await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                                      INSERT INTO "manga_completed_readers" ("MangaId", "UserId")
                                                                      VALUES ({e.MangaId}, {e.UserId})
                                                                      ON CONFLICT ("MangaId", "UserId") DO NOTHING;
                                                                      """, ct);

        if (inserted == 0)
            return;

        await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                       INSERT INTO "manga_reading_stats"
                                                           ("MangaId", "ChaptersRead", "InLists", "Completed")
                                                       VALUES ({e.MangaId}, 0, 0, 1)
                                                       ON CONFLICT ("MangaId") DO UPDATE
                                                       SET "Completed" = "manga_reading_stats"."Completed" + 1;
                                                       """, ct);
    }

    private async Task UnmarkCompleted(ProjectionEvent e, CancellationToken ct)
    {
        var deleted = await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                                     DELETE FROM "manga_completed_readers"
                                                                     WHERE "MangaId" = {e.MangaId} AND "UserId" = {e.UserId};
                                                                     """, ct);

        if (deleted == 0)
            return;

        await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                       UPDATE "manga_reading_stats"
                                                       SET "Completed" =
                                                           CASE WHEN "Completed" > 0 THEN "Completed" - 1 ELSE 0 END
                                                       WHERE "MangaId" = {e.MangaId};
                                                       """, ct);
    }

    public async Task ResetAsync(CancellationToken ct = default)
    {
        await db.MangaCompletedReaderRows.ExecuteDeleteAsync(ct);
        await db.MangaReadingStats.ExecuteDeleteAsync(ct);
    }
}
