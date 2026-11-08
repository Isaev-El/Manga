# Сборка образа выполняется из корня репозитория:
#   docker build -f src/Services/Catalog/Catalog.Api/Dockerfile -t manga-catalog .
# Обычно запускать его вручную не нужно: см. docker-compose.yml в корне.

# ---------- 1. Сборка ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала копируем только .csproj и восстанавливаем пакеты.
# Docker кеширует этот слой: пока зависимости не менялись, restore не повторяется.
COPY src/Services/Catalog/Catalog.Domain/Catalog.Domain.csproj                 src/Services/Catalog/Catalog.Domain/
COPY src/Services/Catalog/Catalog.Application/Catalog.Application.csproj       src/Services/Catalog/Catalog.Application/
COPY src/Services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj src/Services/Catalog/Catalog.Infrastructure/
COPY src/Services/Catalog/Catalog.Api/Catalog.Api.csproj                       src/Services/Catalog/Catalog.Api/
RUN dotnet restore src/Services/Catalog/Catalog.Api/Catalog.Api.csproj

# Теперь весь код сервиса и публикация
COPY src/Services/Catalog/ src/Services/Catalog/
RUN dotnet publish src/Services/Catalog/Catalog.Api/Catalog.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- 2. Запуск ----------
# В итоговом образе только среда выполнения, без SDK: он в несколько раз меньше
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Приложение слушает порт 8080 (стандарт для образов .NET)
EXPOSE 8080

# Запуск не от root: пользователь app уже есть в образе
USER $APP_UID

ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
