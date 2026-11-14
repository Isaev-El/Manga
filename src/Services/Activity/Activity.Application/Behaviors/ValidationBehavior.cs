using FluentValidation;
using MediatR;

namespace Activity.Application.Behaviors;

/// <summary>
/// Звено конвейера MediatR: перед каждым обработчиком запускает все валидаторы запроса.
/// Есть ошибки — ValidationException (400), обработчик не вызывается.
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
