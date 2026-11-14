using Activity.Application.Abstractions;
using Activity.Domain;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Library;

public record AddToListCommand(string UserId, Guid MangaId, ListStatus Status) : IRequest;

public class AddToListCommandValidator : AbstractValidator<AddToListCommand>
{
    public AddToListCommandValidator()
    {
        RuleFor(c => c.UserId).ValidUserId();
        RuleFor(c => c.MangaId).NotEmpty().WithMessage("Не указан тайтл");
        RuleFor(c => c.Status).IsInEnum().WithMessage("Неизвестный список");
    }
}

public class AddToListCommandHandler(IReadingEntryRepository entries, ICatalogClient catalog)
    : IRequestHandler<AddToListCommand>
{
    public async Task Handle(AddToListCommand command, CancellationToken ct)
    {
        // Тайтл живёт в другом сервисе — спрашиваем Catalog по HTTP
        if (!await catalog.MangaExistsAsync(command.MangaId, ct))
            throw new NotFoundException($"Тайтл {command.MangaId} не найден");

        var entry = await entries.LoadAsync(command.UserId, command.MangaId, ct); // проиграли историю
        entry.AddToList(command.Status);                                          // правило + новое событие
        await entries.SaveAsync(entry, ct);                                       // дописали в конец потока
    }
}
