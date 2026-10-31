namespace Catalog.Application;

/// <summary>
/// Запрошенный объект не найден. API превращает это исключение в 404 Not Found.
/// </summary>
public class NotFoundException(string message) : Exception(message);
