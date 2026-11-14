using Catalog.Application.Abstractions;
using Catalog.Domain;
using FluentValidation;
using MediatR;

namespace Catalog.Application.Features.Mangas;

// Команда: что хотим сделать и с какими данными. Возвращает id созданной манги.
public record CreateMangaCommand(string Title, string Author, IReadOnlyList<Guid> GenreIds) : IRequest<Guid>;

// Валидатор: проверяет форму данных. Вызывается автоматически ValidationBehavior до обработчика.
public class CreateMangaCommandValidator : AbstractValidator<CreateMangaCommand>
{
    public CreateMangaCommandValidator()
    {
        // 200 — как длина столбца в базе (MangaConfiguration), иначе база отклонит запись
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Название обязательно")
            .MaximumLength(200).WithMessage("Название не длиннее 200 символов");

        RuleFor(c => c.Author)
            .NotEmpty().WithMessage("Автор обязателен")
            .MaximumLength(200).WithMessage("Имя автора не длиннее 200 символов");

        RuleFor(c => c.GenreIds)
            .NotNull().WithMessage("Список жанров обязателен");

        RuleForEach(c => c.GenreIds)
            .NotEqual(Guid.Empty).WithMessage("Пустой идентификатор жанра");
    }
}

// Обработчик: сам сценарий. Сюда запрос приходит уже провалидированным.
public class CreateMangaCommandHandler(
    IMangaRepository mangas,
    IGenreRepository genres,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateMangaCommand, Guid>
{
    public async Task<Guid> Handle(CreateMangaCommand command, CancellationToken ct)
    {
        var found = await genres.GetByIdsAsync(command.GenreIds, ct);
        if (found.Count != command.GenreIds.Distinct().Count())
            throw new NotFoundException("Некоторые жанры не найдены");

        var manga = Manga.Create(command.Title, command.Author);
        foreach (var genre in found)
            manga.AddGenre(genre);

        mangas.Add(manga);
        await unitOfWork.SaveChangesAsync(ct); // одна точка сохранения

        return manga.Id;
    }
}
