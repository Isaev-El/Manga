using Activity.Application.Abstractions;
using MediatR;
using FluentValidation;

namespace Activity.Application.Features.Stats;

public record GetReadingStatsQuery(DateOnly From, DateOnly To) : IRequest<IReadOnlyList<DailyReadingStatsDto>>;

public class GetReadingStatsQueryValidator : AbstractValidator<GetReadingStatsQuery>
{
    public GetReadingStatsQueryValidator()
    {
        RuleFor(q => q.From)
            .Must((q, from) => from <= q.To)
            .WithMessage("Дата начала не может быть позже даты окончания");

        RuleFor(q => q.From)
            .Must((q, from) => q.To.DayNumber - from.DayNumber + 1 <= 90)
            .When(q => q.From <= q.To)
            .WithMessage("Период не должен превышать 90 дней");
    }
}

public class GetReadingStatsQueryHandler(IReadingStatsReadModel readModel)
    : IRequestHandler<GetReadingStatsQuery, IReadOnlyList<DailyReadingStatsDto>>
{
    public Task<IReadOnlyList<DailyReadingStatsDto>> Handle(
        GetReadingStatsQuery query, CancellationToken ct) =>
        readModel.GetReadingStatsAsync(query.From, query.To, ct);
}
