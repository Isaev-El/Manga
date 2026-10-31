namespace Catalog.Domain.Common;

/// <summary>
/// Базовый класс сущности. Сущность не сохраняет себя и ничего не отправляет:
/// она только записывает, что с ней произошло (Raise). Что делать с событиями,
/// решает инфраструктура (Unit of Work кладёт их в outbox).
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
