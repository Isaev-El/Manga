namespace Activity.Application.Abstractions;

/// <summary>
/// Доступ к сервису Catalog. Activity не лезет в чужую базу — только через API.
/// Интерфейс здесь, реализация (HttpClient) — в Infrastructure.
/// </summary>
public interface ICatalogClient
{
    Task<bool> MangaExistsAsync(Guid mangaId, CancellationToken ct = default);

    /// <summary>Глава по id или null, если такой нет.</summary>
    Task<CatalogChapter?> GetChapterAsync(Guid chapterId, CancellationToken ct = default);
}

/// <summary>То, что Activity нужно знать о главе из Catalog.</summary>
public record CatalogChapter(Guid Id, Guid MangaId, decimal Number, string Status)
{
    public bool IsPublished => string.Equals(Status, "Published", StringComparison.OrdinalIgnoreCase);
}
