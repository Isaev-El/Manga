namespace Catalog.Domain.Common;

/// <summary>
/// Доменное событие: факт, который произошёл с сущностью ("тайтл создан", "тайтл завершён").
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
