FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Services/Activity/Activity.Domain/Activity.Domain.csproj                 src/Services/Activity/Activity.Domain/
COPY src/Services/Activity/Activity.Application/Activity.Application.csproj       src/Services/Activity/Activity.Application/
COPY src/Services/Activity/Activity.Infrastructure/Activity.Infrastructure.csproj src/Services/Activity/Activity.Infrastructure/
COPY src/Services/Activity/Activity.Api/Activity.Api.csproj                       src/Services/Activity/Activity.Api/
RUN dotnet restore src/Services/Activity/Activity.Api/Activity.Api.csproj

COPY src/Services/Activity/ src/Services/Activity/
RUN dotnet publish src/Services/Activity/Activity.Api/Activity.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "Activity.Api.dll"]
