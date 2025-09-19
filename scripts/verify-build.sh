#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

cd "${REPO_ROOT}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "[error] dotnet CLI not found. Install .NET 8 SDK before running this script." >&2
  exit 1
fi

SOLUTION="${REPO_ROOT}/NexaCrmSolution.sln"
TEST_PROJECT="${REPO_ROOT}/tests/BlazorWebApp.Tests/BlazorWebApp.Tests.csproj"

if [[ ! -f "${SOLUTION}" ]]; then
  echo "[error] Solution file not found at ${SOLUTION}" >&2
  exit 1
fi

if [[ ! -f "${TEST_PROJECT}" ]]; then
  echo "[warning] Test project file ${TEST_PROJECT} not found. Skipping tests." >&2
  RUN_TESTS=false
else
  RUN_TESTS=true
fi

echo "[1/3] Restoring NuGet packages..."
dotnet restore "${SOLUTION}"
if [[ "${RUN_TESTS}" == true ]]; then
  dotnet restore "${TEST_PROJECT}"
fi

echo "[2/3] Building solution in Release configuration..."
dotnet build "${SOLUTION}" --configuration Release --no-restore
if [[ "${RUN_TESTS}" == true ]]; then
  dotnet build "${TEST_PROJECT}" --configuration Release --no-restore
fi

if [[ "${RUN_TESTS}" == true ]]; then
  echo "[3/3] Running unit tests..."
  dotnet test "${TEST_PROJECT}" --configuration Release --no-build --no-restore
else
  echo "[3/3] Tests skipped."
fi

echo "Build verification completed successfully."
