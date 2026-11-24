@echo off
cd /d "%~dp0"
if "%~1"=="" (
    echo Usage: add-migration.cmd MigrationName [Catalog^|Activity]
    echo Example: add-migration.cmd AddComments Activity
    exit /b 1
)
set SERVICE=%~2
if "%SERVICE%"=="" set SERVICE=Catalog
dotnet tool restore
dotnet ef migrations add %1 --project src\Services\%SERVICE%\%SERVICE%.Infrastructure --startup-project src\Services\%SERVICE%\%SERVICE%.Api --output-dir Migrations
