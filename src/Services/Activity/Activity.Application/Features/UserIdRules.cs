using FluentValidation;

namespace Activity.Application.Features;

internal static class UserIdRules
{
    /// <summary>Пользователь пока приходит заголовком X-User-Id (позже — из JWT).</summary>
    public static IRuleBuilderOptions<T, string> ValidUserId<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Не указан пользователь (заголовок X-User-Id)")
            .MaximumLength(100).WithMessage("Идентификатор пользователя не длиннее 100 символов");
}
