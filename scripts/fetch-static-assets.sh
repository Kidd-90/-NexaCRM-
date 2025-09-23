#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

assets=(
  "https://raw.githubusercontent.com/twbs/icons/v1.11.3/font/fonts/bootstrap-icons.woff2::src/Web/NexaCRM.WebClient/wwwroot/lib/bootstrap-icons/font/bootstrap-icons.woff2"
  "https://raw.githubusercontent.com/twbs/icons/v1.11.3/font/fonts/bootstrap-icons.woff::src/Web/NexaCRM.WebClient/wwwroot/lib/bootstrap-icons/font/bootstrap-icons.woff"
  "https://raw.githubusercontent.com/iconic/open-iconic/master/font/fonts/open-iconic.eot::src/Web/NexaCRM.WebClient/wwwroot/lib/open-iconic/font/open-iconic.eot"
  "https://raw.githubusercontent.com/iconic/open-iconic/master/font/fonts/open-iconic.otf::src/Web/NexaCRM.WebClient/wwwroot/lib/open-iconic/font/open-iconic.otf"
  "https://raw.githubusercontent.com/iconic/open-iconic/master/font/fonts/open-iconic.ttf::src/Web/NexaCRM.WebClient/wwwroot/lib/open-iconic/font/open-iconic.ttf"
  "https://raw.githubusercontent.com/iconic/open-iconic/master/font/fonts/open-iconic.svg::src/Web/NexaCRM.WebClient/wwwroot/lib/open-iconic/font/open-iconic.svg"
  "https://raw.githubusercontent.com/iconic/open-iconic/master/font/fonts/open-iconic.woff::src/Web/NexaCRM.WebClient/wwwroot/lib/open-iconic/font/open-iconic.woff"
)

download() {
  local url="$1"
  local destination="$2"
  local absolute_destination="${REPO_ROOT}/${destination}"
  local destination_dir="$(dirname "${absolute_destination}")"

  mkdir -p "${destination_dir}"

  if [[ -f "${absolute_destination}" ]]; then
    echo "[fetch-assets] Skipping ${destination} (already exists)."
    return
  fi

  echo "[fetch-assets] Downloading ${destination}"
  if command -v curl >/dev/null 2>&1; then
    curl -fL --create-dirs -o "${absolute_destination}.tmp" "${url}"
    mv "${absolute_destination}.tmp" "${absolute_destination}"
  elif command -v wget >/dev/null 2>&1; then
    wget -O "${absolute_destination}.tmp" "${url}"
    mv "${absolute_destination}.tmp" "${absolute_destination}"
  else
    echo "[fetch-assets] Neither curl nor wget is available to download assets." >&2
    exit 1
  fi
}

for entry in "${assets[@]}"; do
  url="${entry%%::*}"
  path="${entry##*::}"
  download "${url}" "${path}"
done

echo "[fetch-assets] All assets are present."
