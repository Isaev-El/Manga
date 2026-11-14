using Catalog.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Catalog.Application.Features.Chapters;

// Возвращает номер добавленной страницы
public record AddPageCommand(Guid ChapterId, string ImageKey) : IRequest<int>;

public class AddPageCommandValidator : AbstractValidator<AddPageCommand>
{
    public AddPageCommandValidator()
    {
        RuleFor(c => c.ImageKey)
            .NotEmpty().WithMessage("Не указан файл страницы")
            .MaximumLength(500).WithMessage("Путь к файлу не длиннее 500 символов");
    }
}

public class AddPageCommandHandler(IChapterRepository chapters, IUnitOfWork unitOfWork)
    : IRequestHandler<AddPageCommand, int>
{
    public async Task<int> Handle(AddPageCommand command, CancellationToken ct)
    {
        var chapter = await chapters.GetByIdAsync(command.ChapterId, ct)
            ?? throw new NotFoundException($"Глава {command.ChapterId} не найдена");

        var number = chapter.AddPage(command.ImageKey); // "только в черновике" проверяет сущность

        await unitOfWork.SaveChangesAsync(ct);
        return number;
    }
}
