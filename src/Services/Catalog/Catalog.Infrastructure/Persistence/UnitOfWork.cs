using System.Text.Json;
using Catalog.Application.Abstractions;
using Catalog.Domain.Common;
using Catalog.Infrastructure.Outbox;

namespace Catalog.Infrastructure.Persistence;

public class UnitOfWork(CatalogDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        // 1. Собираем события со всех сущностей, которые отслеживает EF Core
        var entities = db.ChangeTracker.Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var messages = entities
            .SelectMany(e => e.DomainEvents)
            .Select(e => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = e.GetType().Name,
                Payload = JsonSerializer.Serialize(e, e.GetType()),
                OccurredOnUtc = e.OccurredOnUtc
            })
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        // 2. Кладём их в outbox
        db.OutboxMessages.AddRange(messages);

        // 3. Одна транзакция: сохранятся либо и данные, и события, либо ничего
        await db.SaveChangesAsync(ct);
    }
}
