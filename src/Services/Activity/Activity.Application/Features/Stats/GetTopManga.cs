using Activity.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Activity.Application.Features.Stats;

public record GetTopMangaQuery(int Limit = 10) : IRequest<IReadOnlyList<TopMangaDto>>;

public class GetTopMangaQueryValidator : AbstractValidator<GetTopMangaQuery>
{
    public GetTopMangaQueryValidator()
    {
        RuleFor(x => x.Limit)
            .ValidLimit();
    }
}

public class GetTopMangaQueryHandler(IReadingStatsReadModel readModel)
    : IRequestHandler<GetTopMangaQuery, IReadOnlyList<TopMangaDto>>
{
    public Task<IReadOnlyList<TopMangaDto>> Handle(
        GetTopMangaQuery query, CancellationToken ct) =>
        readModel.GetTopMangaAsync(query.Limit, ct);
}
