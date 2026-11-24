using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Activity.Tests;

public sealed class TestDb : IAsyncDisposable
{
    private readonly string _adminConnectionString;
    private readonly string _databaseName;

    public ActivityDbContext Db { get; }

    private TestDb(
        string adminConnectionString,
        string databaseName,
        ActivityDbContext db)
    {
        _adminConnectionString = adminConnectionString;
        _databaseName = databaseName;
        Db = db;
    }

    public static async Task<TestDb> CreateAsync(string adminConnectionString)
    {
        var databaseName = $"activity_test_{Guid.NewGuid():N}";

        await using (var connection = new NpgsqlConnection(adminConnectionString))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(adminConnectionString)
        {
            Database = databaseName
        };

        var options = new DbContextOptionsBuilder<ActivityDbContext>()
            .UseNpgsql(builder.ConnectionString)
            .Options;

        var db = new ActivityDbContext(options);
        await db.Database.MigrateAsync();

        return new TestDb(adminConnectionString, databaseName, db);
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();

        // Npgsql держит физические соединения в пуле,
        // поэтому перед DROP DATABASE пул нужно очистить.
        NpgsqlConnection.ClearAllPools();

        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)";
        await command.ExecuteNonQueryAsync();
    }
}
