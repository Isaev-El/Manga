namespace Activity.Application;

/// <summary>Не найдено. API отвечает 404.</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Конфликт с текущими данными (кто-то изменил их одновременно с нами). API отвечает 409.</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>Сервис Catalog не отвечает. API отвечает 503.</summary>
public class CatalogUnavailableException() : Exception("Сервис Catalog недоступен, попробуйте позже");
