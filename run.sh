#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"

echo "[1/3] Restoring tools (dotnet-ef)..."
dotnet tool restore

echo "[2/3] Checking migrations..."
for S in Catalog Activity; do
  if [ ! -d "src/Services/$S/$S.Infrastructure/Migrations" ]; then
    echo "Creating initial migration for $S..."
    dotnet ef migrations add Init \
      --project "src/Services/$S/$S.Infrastructure" \
      --startup-project "src/Services/$S/$S.Api" \
      --output-dir Migrations
  fi
done

echo "[3/3] Starting services..."
echo "  Catalog  API: http://localhost:5100"
echo "  Activity API: http://localhost:5200"
dotnet run --project src/Services/Catalog/Catalog.Api &
CATALOG_PID=$!
trap 'kill $CATALOG_PID 2>/dev/null' EXIT
dotnet run --project src/Services/Activity/Activity.Api
