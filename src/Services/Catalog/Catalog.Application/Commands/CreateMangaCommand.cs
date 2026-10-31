namespace Catalog.Application.Commands;

public record CreateMangaCommand(string Title, string Author, IReadOnlyCollection<Guid>  GenreIds);
