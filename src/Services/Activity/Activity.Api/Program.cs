using System.Text.Json.Serialization;
using Activity.Application;
using Activity.Domain.Common;
using Activity.Infrastructure;
using Activity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); // "Reading", а не 0
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Activity")
        ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Activity"),
    builder.Configuration["Services:CatalogUrl"]
        ?? throw new InvalidOperationException("Не задан адрес Catalog: Services:CatalogUrl"));

var app = builder.Build();

// Применяем миграции при старте: база manga_activity и таблицы создаются сами
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ActivityDbContext>();
    await db.Database.MigrateAsync();
}

// Исключения -> понятные HTTP-коды вместо 500
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;

    if (error is FluentValidation.ValidationException validationException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            errors = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray())
        });
        return;
    }

    context.Response.StatusCode = error switch
    {
        DomainException => StatusCodes.Status400BadRequest,
        NotFoundException => StatusCodes.Status404NotFound,
        ConflictException => StatusCodes.Status409Conflict,
        CatalogUnavailableException => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError
    };

    await context.Response.WriteAsJsonAsync(new { error = error?.Message });
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5200/scalar
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
}

app.MapControllers();

app.Run();
