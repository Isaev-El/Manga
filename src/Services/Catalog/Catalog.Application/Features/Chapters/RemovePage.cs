using Catalog.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Catalog.Application.Features.Chapters;

public record RemovePageCommand(Guid ChapterId, int PageNumber) : IRequest;

public class RemovePageCommandValidator : AbstractValidator<RemovePageCommand>
{
    public RemovePageCommandValidator()
    {
        RuleFor(c => c.PageNumber)
            .GreaterThan(0).WithMessage("Номер страницы должен быть больше нуля");
    }
}

public class RemovePageCommandHandler(IChapterRepository chapters, IUnitOfWork unitOfWork)
    : IRequestHandler<RemovePageCommand>
{
    public async Task Handle(RemovePageCommand command, CancellationToken ct)
    {
        var chapter = await chapters.GetByIdAsync(command.ChapterId, ct)
            ?? throw new NotFoundException($"Глава {command.ChapterId} не найдена");

        chapter.RemovePage(command.PageNumber);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
