using Catalog.Application.Commands;
using FluentValidation;

namespace Catalog.Application.Validators;

public class CreateMangaCommandValidator : AbstractValidator<CreateMangaCommand>
{
    public CreateMangaCommandValidator()
    {
        RuleFor(m => m.Title)
            .NotEmpty()
            .WithMessage("Название обязательно")
            .MaximumLength(256)
            .WithMessage("Максимальное кол-во символов 256");
        
        RuleFor(m => m.Author)
            .NotEmpty()
            .WithMessage("Автор обязателен")
            .MaximumLength(256)
            .WithMessage("Максимальное кол-во символов 256");

        RuleFor(m => m.GenreIds)
            .NotNull();

        RuleForEach(m => m.GenreIds)
            .NotEqual(Guid.Empty).WithMessage("Пустой идентификатор жанра!");
    }
}