using Activity.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Comments;

public record EditCommentCommand(string UserId, Guid CommentId, string Text) : IRequest;

public class EditCommentCommandValidator : AbstractValidator<EditCommentCommand>
{
    public EditCommentCommandValidator()
    {
        RuleFor(c => c.UserId).ValidUserId();
        RuleFor(c => c.CommentId).NotEmpty().WithMessage("Не указан комментарий");
        RuleFor(c => c.Text)
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Комментарий не может быть пустым")
            .MaximumLength(2000).WithMessage("Комментарий не длиннее 2000 символов");
    }
}

public class EditCommentCommandHandler(IChapterCommentRepository comments)
    : IRequestHandler<EditCommentCommand>
{
    public async Task Handle(EditCommentCommand command, CancellationToken ct)
    {
        var comment = await comments.LoadAsync(command.CommentId, ct)
            ?? throw new NotFoundException($"Комментарий {command.CommentId} не найден");

        if (!comment.Edit(command.UserId, command.Text))
            return; // текст не изменился — 204 без записи в БД

        await comments.SaveAsync(comment, ct);
    }
}
