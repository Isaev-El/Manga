using Activity.Application.Abstractions;
using Activity.Domain;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Library;

/// <summary>Мои списки. Читает готовую проекцию — без проигрывания событий, поэтому быстро.</summary>
public record GetLibraryQuery(string UserId, ListStatus? Status) : IRequest<IReadOnlyList<LibraryEntryDto>>;

public class GetLibraryQueryValidator : AbstractValidator<GetLibraryQuery>
{
    public GetLibraryQueryValidator()
    {
        RuleFor(q => q.UserId).ValidUserId();
        RuleFor(q => q.Status).IsInEnum().When(q => q.Status is not null).WithMessage("Неизвестный список");
    }
}

public class GetLibraryQueryHandler(ILibraryReadModel readModel)
    : IRequestHandler<GetLibraryQuery, IReadOnlyList<LibraryEntryDto>>
{
    public Task<IReadOnlyList<LibraryEntryDto>> Handle(GetLibraryQuery query, CancellationToken ct) =>
        readModel.GetLibraryAsync(query.UserId, query.Status, ct);
}
