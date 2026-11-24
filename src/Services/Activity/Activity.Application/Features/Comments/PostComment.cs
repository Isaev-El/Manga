using Activity.Application.Abstractions;
using Activity.Domain;
using Activity.Domain.Common;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Comments;

public record PostCommentCommand(
    string UserId, Guid ChapterId, string Text, Guid? ParentCommentId = null) : IRequest<Guid>;

public class PostCommentCommandValidator : AbstractValidator<PostCommentCommand>
{
    public PostCommentCommandValidator()
    {
        RuleFor(c => c.UserId).ValidUserId();
        RuleFor(c => c.ChapterId).NotEmpty().WithMessage("Не указана глава");
        RuleFor(c => c.Text)
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Комментарий не может быть пустым")
            .MaximumLength(2000).WithMessage("Комментарий не длиннее 2000 символов");

        RuleFor(c => c.ParentCommentId)
            .Must(id => id != Guid.Empty).When(c => c.ParentCommentId.HasValue)
            .WithMessage("Не указан родительский комментарий");
    }
}

public class PostCommentCommandHandler(
    IChapterCommentRepository comments,
    ICatalogClient catalog) : IRequestHandler<PostCommentCommand, Guid>
{
    public async Task<Guid> Handle(PostCommentCommand command, CancellationToken ct)
    {
        // Глава, её номер и статус живут в Catalog — проверяем так же, как в MarkChapterRead
        var chapter = await catalog.GetChapterAsync(command.ChapterId, ct)
            ?? throw new NotFoundException($"Глава {command.ChapterId} не найдена");

        if (!chapter.IsPublished)
            throw new DomainException("Глава ещё не опубликована");

        // Ответ на комментарий: только первый уровень вложенности и только в этой же главе
        if (command.ParentCommentId is { } parentId)
        {
            var parent = await comments.LoadAsync(parentId, ct)
                ?? throw new NotFoundException($"Родительский комментарий {parentId} не найден");

            if (parent.ChapterId != chapter.Id)
                throw new DomainException("Родительский комментарий относится к другой главе");

            if (parent.ParentCommentId is not null)
                throw new DomainException("Второй уровень вложенности комментариев запрещён");
        }

        // MangaId берём из Catalog, а не из тела запроса
        var comment = ChapterComment.Post(
            chapter.Id, chapter.MangaId, command.UserId, command.Text, command.ParentCommentId);

        await comments.SaveAsync(comment, ct);
        return comment.Id;
    }
}
