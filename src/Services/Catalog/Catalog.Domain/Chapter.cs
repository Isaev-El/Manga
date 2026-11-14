using Catalog.Domain.Common;
using Catalog.Domain.Events;

namespace Catalog.Domain;

/// <summary>
/// Глава манги. Отдельная сущность со своим репозиторием, а не список внутри Manga:
/// иначе ради одной страницы пришлось бы загружать мангу со всеми главами.
/// С мангой глава связана только через MangaId.
/// </summary>
public class Chapter : Entity
{
    private readonly List<Page> _pages = new();

    public Guid MangaId { get; private set; }
    public decimal Number { get; private set; }      // бывают главы 10.5
    public string? Title { get; private set; }
    public ChapterStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public IReadOnlyCollection<Page> Pages => _pages;

    private Chapter() { } // нужен EF Core

    public static Chapter CreateDraft(Guid mangaId, decimal number, string? title)
    {
        if (mangaId == Guid.Empty)
            throw new DomainException("Не указан тайтл");
        if (number <= 0)
            throw new DomainException("Номер главы должен быть больше нуля");

        return new Chapter
        {
            Id = Guid.NewGuid(),
            MangaId = mangaId,
            Number = number,
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            Status = ChapterStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>Добавляет страницу в конец главы и возвращает её номер.</summary>
    public int AddPage(string imageKey)
    {
        EnsureDraft();

        if (string.IsNullOrWhiteSpace(imageKey))
            throw new DomainException("Не указан файл страницы");

        var page = new Page(_pages.Count + 1, imageKey.Trim());
        _pages.Add(page);
        return page.Number;
    }

    /// <summary>Удаляет страницу, следующие страницы сдвигаются, чтобы номера шли без пропусков.</summary>
    public void RemovePage(int number)
    {
        EnsureDraft();

        var page = _pages.FirstOrDefault(p => p.Number == number)
            ?? throw new DomainException($"Страницы {number} в главе нет");

        _pages.Remove(page);

        foreach (var next in _pages.Where(p => p.Number > number))
            next.MoveTo(next.Number - 1);
    }

    public void Publish()
    {
        if (Status == ChapterStatus.Published)
            throw new DomainException("Глава уже опубликована");
        if (_pages.Count == 0)
            throw new DomainException("Нельзя опубликовать главу без страниц");

        Status = ChapterStatus.Published;
        PublishedAtUtc = DateTime.UtcNow;

        Raise(new ChapterPublished(Id, MangaId, Number, Title));
    }

    private void EnsureDraft()
    {
        if (Status != ChapterStatus.Draft)
            throw new DomainException("Опубликованную главу менять нельзя");
    }
}
