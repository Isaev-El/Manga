FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Services/Catalog/Catalog.Domain/Catalog.Domain.csproj                 src/Services/Catalog/Catalog.Domain/
COPY src/Services/Catalog/Catalog.Application/Catalog.Application.csproj       src/Services/Catalog/Catalog.Application/
COPY src/Services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj src/Services/Catalog/Catalog.Infrastructure/
COPY src/Services/Catalog/Catalog.Api/Catalog.Api.csproj                       src/Services/Catalog/Catalog.Api/
RUN dotnet restore src/Services/Catalog/Catalog.Api/Catalog.Api.csproj

COPY src/Services/Catalog/ src/Services/Catalog/
RUN dotnet publish src/Services/Catalog/Catalog.Api/Catalog.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app/logs \
    && chown $APP_UID:$APP_UID /app/logs

COPY --from=build /app/publish .

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
