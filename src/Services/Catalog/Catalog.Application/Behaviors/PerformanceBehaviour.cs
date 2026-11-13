using System.Diagnostics;
using System.Globalization;
using MediatR;
using Microsoft.Extensions.Logging;
namespace Catalog.Application.Behaviors;




/// <summary>
/// Звено конвейера MediatR: срабатывает перед КАЖДЫМ обработчиком.
/// Если для запроса есть валидаторы — запускает их все и собирает ошибки.
/// Есть ошибки — бросает ValidationException (API превращает её в 400), обработчик не вызывается.
/// Нет валидаторов или ошибок — передаёт запрос дальше (next).
///
/// Так обработчикам не нужно вызывать валидацию вручную: она происходит сама.
/// </summary>
public class PerformanceBehaviour<TRequest, TResponse>(ILoggerFactory loggerFactory)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    
    private const int ThresholdMilliseconds = 500;
    private readonly ILogger _logger = loggerFactory.CreateLogger("SlowOperations");
    
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var timer = Stopwatch.StartNew();

        try
        {
            return await next();
        }
        finally 
        {
            timer.Stop();

            if (timer.ElapsedMilliseconds > ThresholdMilliseconds)
            {
                var seconds = timer.Elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
            
                _logger.LogWarning("Операция: - {Operation}, время выполнения {Seconds}с.", typeof(TRequest).Name, seconds);
            }
        }
    }
}