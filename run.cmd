@echo off
cd /d "%~dp0"

echo [1/3] Restoring tools (dotnet-ef)...
dotnet tool restore
if errorlevel 1 goto :error

echo [2/3] Checking migrations...
for %%S in (Catalog Activity) do (
    if not exist "src\Services\%%S\%%S.Infrastructure\Migrations" (
        echo Creating initial migration for %%S...
        dotnet ef migrations add Init --project src\Services\%%S\%%S.Infrastructure --startup-project src\Services\%%S\%%S.Api --output-dir Migrations
        if errorlevel 1 goto :error
    )
)

echo [3/3] Starting services...
echo   Catalog  API: http://localhost:5100  (separate window)
echo   Activity API: http://localhost:5200
start "Catalog API" dotnet run --project src\Services\Catalog\Catalog.Api
dotnet run --project src\Services\Activity\Activity.Api
goto :eof

:error
echo.
echo Something went wrong. Read the error message above.
pause
