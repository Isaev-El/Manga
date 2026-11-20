namespace Activity.Domain.Common;

public abstract record DomainEvent
{
    public DateTime OccurredOnUtc { get; init; } =  DateTime.UtcNow;
}