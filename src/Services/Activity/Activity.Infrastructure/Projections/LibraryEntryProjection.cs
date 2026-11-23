using Activity.Application.Abstractions;
using Activity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activity.Infrastructure.Projections;

public sealed class LibraryEntryProjection(ActivityDbContext db) : IProjection
{
    public async Task ApplyAsync(ProjectionEvent e, CancellationToken ct = default)
    {
        var row = await db.LibraryEntries.FindAsync(
            new object?[] { e.UserId, e.MangaId }, ct);

        if (row is null)
        {
            row = new LibraryEntryRow()
            {
                UserId = e.UserId,
                MangaId = e.MangaId
            };
            db.LibraryEntries.Add(row);
        }

        LibraryEntryProjector.Apply(row, e.Event, e.Version);
    }

    public Task ResetAsync(CancellationToken ct = default) =>
        db.LibraryEntries.ExecuteDeleteAsync(ct);
}
