using Activity.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Comments;

public record DeleteCommentCommand(string UserId, Guid CommentId) : IRequest;

public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(c => c.UserId).ValidUserId();
        RuleFor(c => c.CommentId).NotEmpty().WithMessage("Не указан комментарий");
    }
}

public class DeleteCommentCommandHandler(IChapterCommentRepository comments)
    : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand command, CancellationToken ct)
    {
        var comment = await comments.LoadAsync(command.CommentId, ct)
            ?? throw new NotFoundException($"Комментарий {command.CommentId} не найден");

        if (!comment.Delete(command.UserId))
            return; // уже удалён — 204 без записи в БД

        await comments.SaveAsync(comment, ct);
    }
}
