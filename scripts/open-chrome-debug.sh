#!/usr/bin/env zsh
# Open Chrome with a fresh user profile to avoid cached assets.
# Usage: ./scripts/open-chrome-debug.sh [url]

URL=${1:-http://localhost:5000}
PROFILE_DIR="/tmp/nexacrm-chrome-debug-profile"
CACHE_DIR="$PROFILE_DIR/Cache"

# Remove any previous profile (this clears cache and storage)
rm -rf "$PROFILE_DIR"
mkdir -p "$CACHE_DIR"

# Locate Chrome binary (macOS common path, fallbacks)
CHROME_BIN=""
if [ -x "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" ]; then
  CHROME_BIN="/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
elif command -v google-chrome >/dev/null 2>&1; then
  CHROME_BIN="$(command -v google-chrome)"
elif command -v chromium >/dev/null 2>&1; then
  CHROME_BIN="$(command -v chromium)"
fi

if [ -z "$CHROME_BIN" ]; then
  echo "Chrome/Chromium not found. Please install Chrome or adjust the script path."
  exit 1
fi

# Launch Chrome with the temporary profile; this ensures no cached files are used.
# Flags: --no-first-run avoids welcome screens, --disable-extensions for reproducibility,
# --disk-cache-dir points to the fresh cache dir, --user-data-dir isolates profile.
"$CHROME_BIN" \
  --user-data-dir="$PROFILE_DIR" \
  --disk-cache-dir="$CACHE_DIR" \
  --no-first-run \
  --disable-extensions \
  --disable-translate \
  --incognito \
  --new-window "$URL" &

exit 0
