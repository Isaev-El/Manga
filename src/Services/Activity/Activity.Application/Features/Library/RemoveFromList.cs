using Activity.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Library;

public record RemoveFromListCommand(string UserId, Guid MangaId) : IRequest;

public class RemoveFromListCommandValidator : AbstractValidator<RemoveFromListCommand>
{
    public RemoveFromListCommandValidator()
    {
        RuleFor(c => c.UserId).ValidUserId();
        RuleFor(c => c.MangaId).NotEmpty().WithMessage("Не указан тайтл");
    }
}

public class RemoveFromListCommandHandler(IReadingEntryRepository entries)
    : IRequestHandler<RemoveFromListCommand>
{
    public async Task Handle(RemoveFromListCommand command, CancellationToken ct)
    {
        var entry = await entries.LoadAsync(command.UserId, command.MangaId, ct);
        entry.RemoveFromList();
        await entries.SaveAsync(entry, ct);
    }
}
