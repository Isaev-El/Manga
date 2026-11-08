namespace Catalog.Application;

/// <summary>
/// Действие противоречит уже существующим данным (например, глава с таким номером уже есть).
/// API превращает это исключение в 409 Conflict.
/// </summary>
public class ConflictException(string message) : Exception(message);
