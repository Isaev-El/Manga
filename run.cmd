@echo off
cd /d "%~dp0"

echo [1/3] Restoring tools (dotnet-ef)...
dotnet tool restore
if errorlevel 1 goto :error

if exist "src\Services\Catalog\Catalog.Infrastructure\Persistence\Migrations" (
    echo [2/3] Migrations already exist, skipping.
) else (
    echo [2/3] Creating initial migration...
    dotnet ef migrations add Init --project src\Services\Catalog\Catalog.Infrastructure --startup-project src\Services\Catalog\Catalog.Api --output-dir Persistence\Migrations
    if errorlevel 1 goto :error
)

echo [3/3] Starting Catalog API on http://localhost:5100 ...
dotnet run --project src\Services\Catalog\Catalog.Api
goto :eof

:error
echo.
echo Something went wrong. Read the error message above.
pause
