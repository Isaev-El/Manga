using Catalog.Application.Abstractions;
using Catalog.Domain;

namespace Catalog.Application;

public class MangaService(IMangaRepository mangas, IGenreRepository genres, IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<MangaDto>> ListAsync(Guid? genreId, CancellationToken ct)
    {
        var items = await mangas.ListAsync(genreId, ct);
        return items.Select(m => m.ToDto()).ToList();
    }

    public async Task<MangaDto> GetAsync(Guid id, CancellationToken ct)
    {
        var manga = await mangas.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Тайтл {id} не найден");

        return manga.ToDto();
    }

    public async Task<Guid> CreateAsync(
        string title, string author, IReadOnlyList<Guid> genreIds, CancellationToken ct)
    {
        var manga = Manga.Create(title, author);

        var found = await genres.GetByIdsAsync(genreIds, ct);
        if (found.Count != genreIds.Distinct().Count())
            throw new NotFoundException("Некоторые жанры не найдены");

        foreach (var genre in found)
            manga.AddGenre(genre);

        mangas.Add(manga);
        await unitOfWork.SaveChangesAsync(ct); // одна точка сохранения

        return manga.Id;
    }

    public async Task CompleteAsync(Guid id, CancellationToken ct)
    {
        var manga = await mangas.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Тайтл {id} не найден");

        manga.Complete();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
