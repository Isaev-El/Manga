#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"

echo "[1/3] Restoring tools (dotnet-ef)..."
dotnet tool restore

if [ -d "src/Services/Catalog/Catalog.Infrastructure/Persistence/Migrations" ]; then
  echo "[2/3] Migrations already exist, skipping."
else
  echo "[2/3] Creating initial migration..."
  dotnet ef migrations add Init \
    --project src/Services/Catalog/Catalog.Infrastructure \
    --startup-project src/Services/Catalog/Catalog.Api \
    --output-dir Persistence/Migrations
fi

echo "[3/3] Starting Catalog API on http://localhost:5100 ..."
dotnet run --project src/Services/Catalog/Catalog.Api
