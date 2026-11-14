using Catalog.Application;
using Catalog.Domain.Common;
using Catalog.Infrastructure;
using Catalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend",
            policy => policy.WithOrigins("http://localhost:5100").AllowAnyHeader().AllowAnyMethod()
        );
    }
);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(
        context.Configuration
    );
    
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Catalog")
    ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Catalog"));

var app = builder.Build();

app.UseCors("Frontend");

// Применяем миграции при старте: база и таблицы создаются сами, достаточно dotnet run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.MigrateAsync();
}

// Исключения -> понятные HTTP-коды вместо 500
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;

    if (error is FluentValidation.ValidationException validationException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = validationException.Errors.GroupBy(x => x.PropertyName)
            .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray()) });
        return; // ответ уже записан, второй раз писать нельзя
    }

    context.Response.StatusCode = error switch
    {
        DomainException => StatusCodes.Status400BadRequest,
        NotFoundException => StatusCodes.Status404NotFound,
        ConflictException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    await context.Response.WriteAsJsonAsync(new { error = error?.Message });
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5100/scalar
}

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();
