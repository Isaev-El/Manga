using Activity.Application;
using Activity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Activity.Tests;

[Collection(PostgresCollection.Name)]
public class StatsProjectionTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Repeat_reading_counts_chapters_but_not_active_readers()
    {
        await using var testDb = await TestDb.CreateAsync(fixture.ConnectionString);
        var db = testDb.Db;
        var repository = TestServices.CreateRepository(db);
        var readModel = TestServices.CreateReadModel(db);

        var userId = "user-1";
        var mangaId = Guid.NewGuid();

        var entry = ReadingEntry.Load(userId, mangaId, []);
        entry.MarkChapterRead(Guid.NewGuid(), 1);
        await repository.SaveAsync(entry);

        var entry2 = await repository.LoadAsync(userId, mangaId);
        entry2.MarkChapterRead(Guid.NewGuid(), 1);
        await repository.SaveAsync(entry2);

        var occurredOn = await db.Events
            .Where(e => e.StreamId == ReadingEntry.StreamId(userId, mangaId))
            .Select(e => e.OccurredOnUtc)
            .FirstAsync();

        var day = DateOnly.FromDateTime(occurredOn.AddHours(5));

        var stats = await readModel.GetReadingStatsAsync(day, day);
        var row = Assert.Single(stats);

        Assert.Equal(2, row.ChaptersRead);
        Assert.Equal(1, row.ActiveReaders);
        Assert.Equal(1, row.TitlesAdded);

        var top = await readModel.GetTopMangaAsync(10);
        var manga = Assert.Single(top);

        Assert.Equal(2, manga.ChaptersRead);
        Assert.Equal(1, manga.InLists);
    }

    [Fact]
    public async Task Completed_is_decremented_when_removed_from_list()
    {
        await using var testDb = await TestDb.CreateAsync(fixture.ConnectionString);
        var db = testDb.Db;
        var repository = TestServices.CreateRepository(db);
        var readModel = TestServices.CreateReadModel(db);

        var userId = "user-1";
        var mangaId = Guid.NewGuid();

        var entry = ReadingEntry.Load(userId, mangaId, []);
        entry.AddToList(ListStatus.Completed);
        await repository.SaveAsync(entry);

        var top = await readModel.GetTopMangaAsync(10);
        var row = Assert.Single(top);

        Assert.Equal(1, row.InLists);
        Assert.Equal(1, row.Completed);

        var entry2 = await repository.LoadAsync(userId, mangaId);
        entry2.RemoveFromList();
        await repository.SaveAsync(entry2);

        top = await readModel.GetTopMangaAsync(10);
        row = Assert.Single(top);

        Assert.Equal(0, row.InLists);
        Assert.Equal(0, row.Completed);
    }

    [Fact]
    public async Task Reading_stats_returns_zeroes_for_empty_days()
    {
        await using var testDb = await TestDb.CreateAsync(fixture.ConnectionString);
        var readModel = TestServices.CreateReadModel(testDb.Db);

        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 1, 3);

        var stats = await readModel.GetReadingStatsAsync(from, to);

        Assert.Equal(3, stats.Count);
        Assert.All(stats, x => Assert.Equal(0, x.ChaptersRead));
        Assert.Equal(from, stats[0].Day);
        Assert.Equal(to, stats[2].Day);
    }

    [Fact]
    public async Task Drop_off_groups_readers_by_last_chapter()
    {
        await using var testDb = await TestDb.CreateAsync(fixture.ConnectionString);
        var db = testDb.Db;
        var repository = TestServices.CreateRepository(db);
        var readModel = TestServices.CreateReadModel(db);

        var mangaId = Guid.NewGuid();

        var user1 = ReadingEntry.Load("user-1", mangaId, []);
        user1.MarkChapterRead(Guid.NewGuid(), 1);
        await repository.SaveAsync(user1);

        var user2 = ReadingEntry.Load("user-2", mangaId, []);
        user2.MarkChapterRead(Guid.NewGuid(), 2);
        await repository.SaveAsync(user2);

        var dropOff = await readModel.GetDropOffAsync(mangaId);

        Assert.Equal(2, dropOff.Count);
        Assert.Equal(1m, dropOff[0].ChapterNumber);
        Assert.Equal(1, dropOff[0].ReadersStopped);
        Assert.Equal(2m, dropOff[1].ChapterNumber);
        Assert.Equal(1, dropOff[1].ReadersStopped);
    }

    [Fact]
    public async Task Concurrent_write_throws_conflict()
    {
        await using var testDb = await TestDb.CreateAsync(fixture.ConnectionString);
        var db = testDb.Db;
        var repository = TestServices.CreateRepository(db);

        var mangaId = Guid.NewGuid();

        var first = ReadingEntry.Load("user-1", mangaId, []);
        first.AddToList(ListStatus.Reading);

        var second = ReadingEntry.Load("user-1", mangaId, []);
        second.AddToList(ListStatus.Planned);

        await repository.SaveAsync(first);

        await Assert.ThrowsAsync<ConflictException>(
            () => repository.SaveAsync(second));
    }
}
