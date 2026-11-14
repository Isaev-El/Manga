using Activity.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Library;

public record RateMangaCommand(string UserId, Guid MangaId, int Score) : IRequest;

public class RateMangaCommandValidator : AbstractValidator<RateMangaCommand>
{
    public RateMangaCommandValidator()
    {
        RuleFor(c => c.UserId).ValidUserId();
        RuleFor(c => c.MangaId).NotEmpty().WithMessage("Не указан тайтл");
        RuleFor(c => c.Score).InclusiveBetween(1, 10).WithMessage("Оценка должна быть от 1 до 10");
    }
}

public class RateMangaCommandHandler(IReadingEntryRepository entries)
    : IRequestHandler<RateMangaCommand>
{
    public async Task Handle(RateMangaCommand command, CancellationToken ct)
    {
        var entry = await entries.LoadAsync(command.UserId, command.MangaId, ct);
        entry.Rate(command.Score); // правило "только тайтлу из списка" — в агрегате
        await entries.SaveAsync(entry, ct);
    }
}
