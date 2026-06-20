#!/usr/bin/env bash
set -euo pipefail

BASE_DIR=/root/task
cd "$BASE_DIR"

echo "Starting SQL Server datastore..."
docker compose up -d

echo "Waiting for SQL Server to become healthy..."
for i in $(seq 1 30); do
  status=$(docker inspect --format='{{.State.Health.Status}}' iot_sqlserver 2>/dev/null || echo "starting")
  if [ "$status" = "healthy" ]; then
    echo "SQL Server is healthy."
    break
  fi
  echo "  ... still waiting ($i) [status=$status]"
  sleep 5
done

echo "Building solution..."
dotnet build "$BASE_DIR/IotMonitoring.sln"

echo "Running tests..."
dotnet test "$BASE_DIR/IotMonitoring.sln"

