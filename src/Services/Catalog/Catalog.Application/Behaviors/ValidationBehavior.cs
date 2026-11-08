using FluentValidation;
using MediatR;

namespace Catalog.Application.Behaviors;

/// <summary>
/// Звено конвейера MediatR: срабатывает перед КАЖДЫМ обработчиком.
/// Если для запроса есть валидаторы — запускает их все и собирает ошибки.
/// Есть ошибки — бросает ValidationException (API превращает её в 400), обработчик не вызывается.
/// Нет валидаторов или ошибок — передаёт запрос дальше (next).
///
/// Так обработчикам не нужно вызывать валидацию вручную: она происходит сама.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any())
            return await next();

        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(request, ct)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
