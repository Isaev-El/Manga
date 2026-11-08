using Catalog.Application.Commands;
using FluentValidation;

namespace Catalog.Application.Validators;

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

public class AddPageCommandValidator : AbstractValidator<AddPageCommand>
{
    public AddPageCommandValidator()
    {
        RuleFor(p => p.ImageKey)
            .NotEmpty().WithMessage("Не указан файл страницы")
            .MaximumLength(500).WithMessage("Путь к файлу не длиннее 500 символов");
    }
}
