using Catalog.Domain.Common;
using Catalog.Domain.Events;

namespace Catalog.Domain;

public class Manga : Entity
{
    private readonly List<Genre> _genres = new();

    public string Title { get; private set; } = null!;
    public string Author { get; private set; } = null!;
    public MangaStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<Genre> Genres => _genres;

    private Manga() { } // нужен EF Core

    public static Manga Create(string title, string author)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Название обязательно");
        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Автор обязателен");

        var manga = new Manga
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Author = author.Trim(),
            Status = MangaStatus.Ongoing,
            CreatedAtUtc = DateTime.UtcNow
        };

        manga.Raise(new MangaCreated(manga.Id, manga.Title));
        return manga;
    }

    public void AddGenre(Genre genre)
    {
        if (_genres.Any(g => g.Id == genre.Id))
            return;

        _genres.Add(genre);
    }

    public void Complete()
    {
        if (Status == MangaStatus.Completed)
            throw new DomainException("Тайтл уже завершён");

        Status = MangaStatus.Completed;
        Raise(new MangaCompleted(Id));
    }
}
