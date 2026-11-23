using Activity.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Stats;

public record GetDropOffQuery(Guid MangaId) : IRequest<IReadOnlyList<DropOffDto>>;

public class GetDropOffQueryValidator : AbstractValidator<GetDropOffQuery>
{
    public GetDropOffQueryValidator()
    {
        RuleFor(x => x.MangaId)
            .NotEmpty()
            .WithMessage("Не указан тайтл");
    }
}

public class GetDropOffQueryHandler(IReadingStatsReadModel readModel)
    : IRequestHandler<GetDropOffQuery, IReadOnlyList<DropOffDto>>
{
    public Task<IReadOnlyList<DropOffDto>> Handle(
        GetDropOffQuery query, CancellationToken ct) =>
        readModel.GetDropOffAsync(query.MangaId, ct);
}
