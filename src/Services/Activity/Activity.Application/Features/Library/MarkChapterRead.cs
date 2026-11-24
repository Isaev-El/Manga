using Activity.Application.Abstractions;
using Activity.Domain.Common;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Library;

public record MarkChapterReadCommand(string UserId, Guid MangaId, Guid ChapterId) : IRequest;

public class MarkChapterReadCommandValidator : AbstractValidator<MarkChapterReadCommand>
{
    public MarkChapterReadCommandValidator()
    {
        RuleFor(c => c.UserId).ValidUserId();
        RuleFor(c => c.MangaId).NotEmpty().WithMessage("Не указан тайтл");
        RuleFor(c => c.ChapterId).NotEmpty().WithMessage("Не указана глава");
    }
}

public class MarkChapterReadCommandHandler(IReadingEntryRepository entries, ICatalogClient catalog)
    : IRequestHandler<MarkChapterReadCommand>
{
    public async Task Handle(MarkChapterReadCommand command, CancellationToken ct)
    {
        // Номер главы и её статус знает только Catalog
        var chapter = await catalog.GetChapterAsync(command.ChapterId, ct)
            ?? throw new NotFoundException($"Глава {command.ChapterId} не найдена");

        if (chapter.MangaId != command.MangaId)
            throw new DomainException("Глава не относится к этому тайтлу");

        if (!chapter.IsPublished)
            throw new DomainException("Глава ещё не опубликована");

        var entry = await entries.LoadAsync(command.UserId, command.MangaId, ct);
        entry.MarkChapterRead(chapter.Id, chapter.Number); // если тайтла нет в списках — добавит в "Читаю"
        await entries.SaveAsync(entry, ct);
    }
}
