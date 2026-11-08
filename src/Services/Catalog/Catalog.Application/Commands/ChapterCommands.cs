namespace Catalog.Application.Commands;

public record CreateChapterCommand(Guid MangaId, decimal Number, string? Title);

public record AddPageCommand(string ImageKey);
