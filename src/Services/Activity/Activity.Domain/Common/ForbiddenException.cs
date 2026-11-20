namespace Activity.Domain.Common;

/// <summary>
/// Действие запрещено: пользователь не является автором комментария. API отвечает 403.
/// </summary>
public class ForbiddenException(string message) : Exception(message);
