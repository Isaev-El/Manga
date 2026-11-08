using Catalog.Application.Abstractions;
using Catalog.Application.Commands;
using Catalog.Domain;
using FluentValidation;

namespace Catalog.Application;

public class ChapterService(
    IChapterRepository chapters,
    IMangaRepository mangas,
    IUnitOfWork unitOfWork,
    IValidator<CreateChapterCommand> createValidator,
    IValidator<AddPageCommand> addPageValidator)
{
    public async Task<IReadOnlyList<ChapterDto>> ListAsync(Guid mangaId, CancellationToken ct)
    {
        await EnsureMangaExistsAsync(mangaId, ct);

        var items = await chapters.ListByMangaAsync(mangaId, ct);
        return items.Select(c => c.ToDto()).ToList();
    }

    public async Task<ChapterDetailsDto> GetAsync(Guid id, CancellationToken ct)
    {
        var chapter = await GetChapterAsync(id, ct);
        return chapter.ToDetailsDto();
    }

    public async Task<Guid> CreateAsync(CreateChapterCommand command, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(command, ct);
        await EnsureMangaExistsAsync(command.MangaId, ct);

        // Правило "номер уникален в пределах тайтла" касается нескольких глав сразу,
        // поэтому проверяем его здесь, а не в сущности. Подстраховка — уникальный индекс в базе.
        if (await chapters.ExistsAsync(command.MangaId, command.Number, ct))
            throw new ConflictException($"Глава {command.Number} у этого тайтла уже есть");

        var chapter = Chapter.CreateDraft(command.MangaId, command.Number, command.Title);

        chapters.Add(chapter);
        await unitOfWork.SaveChangesAsync(ct);

        return chapter.Id;
    }

    public async Task<int> AddPageAsync(Guid chapterId, AddPageCommand command, CancellationToken ct)
    {
        await addPageValidator.ValidateAndThrowAsync(command, ct);

        var chapter = await GetChapterAsync(chapterId, ct);
        var number = chapter.AddPage(command.ImageKey);

        await unitOfWork.SaveChangesAsync(ct);
        return number;
    }

    public async Task RemovePageAsync(Guid chapterId, int pageNumber, CancellationToken ct)
    {
        var chapter = await GetChapterAsync(chapterId, ct);
        chapter.RemovePage(pageNumber);

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task PublishAsync(Guid chapterId, CancellationToken ct)
    {
        var chapter = await GetChapterAsync(chapterId, ct);
        chapter.Publish(); // поднимает ChapterPublished -> Unit of Work положит его в outbox

        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Chapter> GetChapterAsync(Guid id, CancellationToken ct) =>
        await chapters.GetByIdAsync(id, ct)
        ?? throw new NotFoundException($"Глава {id} не найдена");

    private async Task EnsureMangaExistsAsync(Guid mangaId, CancellationToken ct)
    {
        if (!await mangas.ExistsAsync(mangaId, ct))
            throw new NotFoundException($"Тайтл {mangaId} не найден");
    }
}
