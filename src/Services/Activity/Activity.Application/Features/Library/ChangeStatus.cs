using Activity.Application.Abstractions;
using Activity.Domain;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Library;

public record ChangeStatusCommand(string UserId, Guid MangaId, ListStatus Status) : IRequest;

public class ChangeStatusCommandValidator : AbstractValidator<ChangeStatusCommand>
{
    public ChangeStatusCommandValidator()
    {
        RuleFor(c => c.UserId).ValidUserId();
        RuleFor(c => c.MangaId).NotEmpty().WithMessage("Не указан тайтл");
        RuleFor(c => c.Status).IsInEnum().WithMessage("Неизвестный список");
    }
}

public class ChangeStatusCommandHandler(IReadingEntryRepository entries)
    : IRequestHandler<ChangeStatusCommand>
{
    public async Task Handle(ChangeStatusCommand command, CancellationToken ct)
    {
        var entry = await entries.LoadAsync(command.UserId, command.MangaId, ct);
        entry.ChangeStatus(command.Status);
        await entries.SaveAsync(entry, ct);
    }
}
