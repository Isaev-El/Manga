using FluentValidation;

namespace Activity.Application.Features.Stats;

public static class StatsRules
{
    public static IRuleBuilderOptions<T, int> ValidLimit<T>(
        this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(1, 100)
            .WithMessage("Лимит должен быть от 1 до 100");
}
