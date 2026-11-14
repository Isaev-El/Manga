using Catalog.Domain;

namespace Catalog.Application;

/// <summary>Глава в списке: без страниц, только их количество.</summary>
public record ChapterDto(
    Guid Id,
    Guid MangaId,
    decimal Number,
    string? Title,
    string Status,
    int PageCount,
    DateTime CreatedAtUtc,
    DateTime? PublishedAtUtc);

public record PageDto(int Number, string ImageKey);

/// <summary>Одна глава целиком: со страницами по порядку.</summary>
public record ChapterDetailsDto(
    Guid Id,
    Guid MangaId,
    decimal Number,
    string? Title,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? PublishedAtUtc,
    IReadOnlyList<PageDto> Pages);

internal static class ChapterMapping
{
    public static ChapterDto ToDto(this Chapter c) => new(
        c.Id, c.MangaId, c.Number, c.Title, c.Status.ToString(),
        c.Pages.Count, c.CreatedAtUtc, c.PublishedAtUtc);

    public static ChapterDetailsDto ToDetailsDto(this Chapter c) => new(
        c.Id, c.MangaId, c.Number, c.Title, c.Status.ToString(),
        c.CreatedAtUtc, c.PublishedAtUtc,
        c.Pages
            .OrderBy(p => p.Number) // из базы страницы приходят без гарантии порядка
            .Select(p => new PageDto(p.Number, p.ImageKey))
            .ToList());
}
