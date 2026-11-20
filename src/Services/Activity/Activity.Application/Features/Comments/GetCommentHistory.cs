using Activity.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Comments;

public record GetCommentHistoryQuery(Guid CommentId)
    : IRequest<IReadOnlyList<CommentHistoryItemDto>>;

public class GetCommentHistoryQueryValidator : AbstractValidator<GetCommentHistoryQuery>
{
    public GetCommentHistoryQueryValidator()
    {
        RuleFor(q => q.CommentId).NotEmpty().WithMessage("Не указан комментарий");
    }
}

public class GetCommentHistoryQueryHandler(ICommentReadModel readModel)
    : IRequestHandler<GetCommentHistoryQuery, IReadOnlyList<CommentHistoryItemDto>>
{
    public async Task<IReadOnlyList<CommentHistoryItemDto>> Handle(
        GetCommentHistoryQuery query, CancellationToken ct)
    {
        var items = await readModel.GetHistoryAsync(query.CommentId, ct);
        if (items.Count == 0)
            throw new NotFoundException($"Комментарий {query.CommentId} не найден");

        return items;
    }
}
