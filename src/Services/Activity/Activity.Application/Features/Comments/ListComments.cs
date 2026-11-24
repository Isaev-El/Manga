using Activity.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Comments;

public record ListCommentsQuery(Guid ChapterId, int Page, int PageSize)
    : IRequest<CommentPageDto>;

public class ListCommentsQueryValidator : AbstractValidator<ListCommentsQuery>
{
    public ListCommentsQueryValidator()
    {
        RuleFor(q => q.ChapterId).NotEmpty().WithMessage("Не указана глава");

        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1).WithMessage("page должна быть больше 0");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 100).WithMessage("pageSize должен быть от 1 до 100");
    }
}

public class ListCommentsQueryHandler(ICommentReadModel readModel)
    : IRequestHandler<ListCommentsQuery, CommentPageDto>
{
    public Task<CommentPageDto> Handle(ListCommentsQuery query, CancellationToken ct) =>
        readModel.GetCommentsAsync(query.ChapterId, query.Page, query.PageSize, ct);
}
