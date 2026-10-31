using Catalog.Domain;

namespace Catalog.Application;

// Наружу отдаём DTO, а не сущности: API не зависит от внутреннего устройства домена.
public record GenreDto(Guid Id, string Name);

public record MangaDto(
    Guid Id,
    string Title,
    string Author,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<GenreDto> Genres);

internal static class Mapping
{
    public static GenreDto ToDto(this Genre genre) => new(genre.Id, genre.Name);

    public static MangaDto ToDto(this Manga manga) => new(
        manga.Id,
        manga.Title,
        manga.Author,
        manga.Status.ToString(),
        manga.CreatedAtUtc,
        manga.Genres.Select(g => g.ToDto()).ToList());
}
