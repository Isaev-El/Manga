using Activity.Domain;
using Activity.Domain.Events;
using Activity.Infrastructure.EventStore;
using Microsoft.EntityFrameworkCore;

namespace Activity.Tests;

[Collection(PostgresCollection.Name)]
public class StatsRebuildTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Rebuild_gives_same_numbers_as_incremental_projection()
    {
        await using var testDb = await TestDb.CreateAsync(fixture.ConnectionString);
        var db = testDb.Db;
        var repository = TestServices.CreateRepository(db);
        var readModel = TestServices.CreateReadModel(db);
        var rebuilder = TestServices.CreateRebuilder(db);

        var userId = "user-1";
        var mangaId = Guid.NewGuid();

        var entry = ReadingEntry.Load(userId, mangaId, []);
        entry.MarkChapterRead(Guid.NewGuid(), 1);
        entry.MarkChapterRead(Guid.NewGuid(), 2);
        entry.ChangeStatus(ListStatus.Completed);
        await repository.SaveAsync(entry);

        var occurredOn = await db.Events
            .Where(e => e.StreamId == ReadingEntry.StreamId(userId, mangaId))
            .Select(e => e.OccurredOnUtc)
            .FirstAsync();

        var day = DateOnly.FromDateTime(occurredOn.AddHours(5));

        var dailyBefore = await readModel.GetReadingStatsAsync(day, day);
        var topBefore = await readModel.GetTopMangaAsync(10);

        var result = await rebuilder.RebuildAsync();

        var dailyAfter = await readModel.GetReadingStatsAsync(day, day);
        var topAfter = await readModel.GetTopMangaAsync(10);

        Assert.Equal(dailyBefore.ToArray(), dailyAfter.ToArray());
        Assert.Equal(topBefore.ToArray(), topAfter.ToArray());
        Assert.Equal(4, result.EventsProcessed);
        Assert.Equal(1, result.Batches);
    }

    [Fact]
    public async Task Rebuild_reads_events_in_batches_of_1000()
    {
        await using var testDb = await TestDb.CreateAsync(fixture.ConnectionString);
        var db = testDb.Db;
        var rebuilder = TestServices.CreateRebuilder(db);

        var mangaId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        for (var i = 0; i < 1001; i++)
        {
            db.Events.Add(new EventRecord
            {
                StreamId = ReadingEntry.StreamId($"user-{i}", mangaId),
                Version = 1,
                Type = nameof(ChapterRead),
                Data = EventSerializer.Serialize(
                    new ChapterRead(Guid.NewGuid(), i + 1)),
                OccurredOnUtc = occurredOn
            });
        }

        await db.SaveChangesAsync();

        var result = await rebuilder.RebuildAsync();

        Assert.Equal(1001, result.EventsProcessed);
        Assert.Equal(2, result.Batches);
    }
}
