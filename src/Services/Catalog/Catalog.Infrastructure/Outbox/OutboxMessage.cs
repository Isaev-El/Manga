namespace Catalog.Infrastructure.Outbox;

/// <summary>
/// Запись в таблице outbox: доменное событие, сохранённое в той же транзакции,
/// что и данные. Позже фоновый процесс прочитает его и отправит в брокер сообщений.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
}
