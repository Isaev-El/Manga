using Activity.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Library;

/// <summary>Вся история действий пользователя с тайтлом — то, что даёт только event sourcing.</summary>
public record GetHistoryQuery(string UserId, Guid MangaId) : IRequest<IReadOnlyList<HistoryItemDto>>;

public class GetHistoryQueryValidator : AbstractValidator<GetHistoryQuery>
{
    public GetHistoryQueryValidator()
    {
        RuleFor(q => q.UserId).ValidUserId();
        RuleFor(q => q.MangaId).NotEmpty().WithMessage("Не указан тайтл");
    }
}

public class GetHistoryQueryHandler(ILibraryReadModel readModel)
    : IRequestHandler<GetHistoryQuery, IReadOnlyList<HistoryItemDto>>
{
    public Task<IReadOnlyList<HistoryItemDto>> Handle(GetHistoryQuery query, CancellationToken ct) =>
        readModel.GetHistoryAsync(query.UserId, query.MangaId, ct);
}
