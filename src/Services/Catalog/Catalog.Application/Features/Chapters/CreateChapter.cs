using Catalog.Application.Abstractions;
using Catalog.Domain;
using FluentValidation;
using MediatR;

namespace Catalog.Application.Features.Chapters;

public record CreateChapterCommand(Guid MangaId, decimal Number, string? Title) : IRequest<Guid>;

public class CreateChapterCommandValidator : AbstractValidator<CreateChapterCommand>
{
    public CreateChapterCommandValidator()
    {
        RuleFor(c => c.MangaId)
            .NotEmpty().WithMessage("Не указан тайтл");

        RuleFor(c => c.Number)
            .GreaterThan(0).WithMessage("Номер главы должен быть больше нуля")
            .PrecisionScale(8, 2, ignoreTrailingZeros: true)
            .WithMessage("Номер главы: не больше двух знаков после запятой");

        RuleFor(c => c.Title)
            .MaximumLength(200).WithMessage("Название главы не длиннее 200 символов");
    }
}

public class CreateChapterCommandHandler(
    IChapterRepository chapters,
    IMangaRepository mangas,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateChapterCommand, Guid>
{
    public async Task<Guid> Handle(CreateChapterCommand command, CancellationToken ct)
    {
        if (!await mangas.ExistsAsync(command.MangaId, ct))
            throw new NotFoundException($"Тайтл {command.MangaId} не найден");

        // Правило "номер уникален в пределах тайтла" касается нескольких глав сразу,
        // поэтому проверяем его здесь, а не в сущности. Подстраховка — уникальный индекс в базе.
        if (await chapters.ExistsAsync(command.MangaId, command.Number, ct))
            throw new ConflictException($"Глава {command.Number} у этого тайтла уже есть");

        var chapter = Chapter.CreateDraft(command.MangaId, command.Number, command.Title);

        chapters.Add(chapter);
        await unitOfWork.SaveChangesAsync(ct);

        return chapter.Id;
    }
}
