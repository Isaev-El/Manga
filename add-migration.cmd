@echo off
cd /d "%~dp0"
if "%~1"=="" (
    echo Usage: add-migration.cmd MigrationName
    exit /b 1
)
dotnet tool restore
dotnet ef migrations add %1 --project src\Services\Catalog\Catalog.Infrastructure --startup-project src\Services\Catalog\Catalog.Api --output-dir Migrations
