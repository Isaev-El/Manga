namespace Catalog.Domain.Common;

/// <summary>
/// Нарушено бизнес-правило. API превращает это исключение в 400 Bad Request.
/// </summary>
public class DomainException(string message) : Exception(message);
