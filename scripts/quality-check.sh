#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"

cd "$REPO_ROOT"

echo "[quality-check] Running dotnet build..."
dotnet build --configuration Release

echo "[quality-check] Running solution tests..."
dotnet test --configuration Release

echo "[quality-check] Running Blazor WebApp tests..."
dotnet test ./tests/BlazorWebApp.Tests --configuration Release

python3 "$SCRIPT_DIR/check_console.py"
