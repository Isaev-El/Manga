using Microsoft.EntityFrameworkCore;

namespace Activity.Tests;

[Collection(PostgresCollection.Name)]
public class MigrationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Events_table_has_brin_index_on_occurred_on_utc()
    {
        await using var testDb = await TestDb.CreateAsync(fixture.ConnectionString);

        var method = await testDb.Db.Database
            .SqlQuery<string>($"""
                SELECT am.amname::text AS "Value"
                FROM pg_class c
                JOIN pg_index i ON i.indexrelid = c.oid
                JOIN pg_am am ON am.oid = c.relam
                WHERE c.relname = 'IX_events_OccurredOnUtc'
                """)
            .SingleAsync();

        Assert.Equal("brin", method);
    }
}
