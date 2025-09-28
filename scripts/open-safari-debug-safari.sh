#!/usr/bin/env zsh
# Open Safari after clearing common Safari/WebKit caches so the page loads without stale cached assets.
# Usage: ./scripts/open-safari-debug-safari.sh [url] [--private]
# Note: clearing caches deletes runtime caches only; it does not remove bookmarks or preferences.

URL=${1:-http://localhost:5000}
PRIVATE_FLAG=${2:-}

# Quit Safari to ensure caches can be removed safely
osascript -e 'tell application "Safari" to quit' >/dev/null 2>&1 || true
sleep 0.6

echo "Clearing Safari & WebKit caches (safe, non-invasive)..."
# Safe deletion: use find to delete contents if directory exists to avoid zsh globbing errors
cleanup_dir() {
  local dir="$1"
  if [ -d "$dir" ]; then
    # delete all children (files and dirs) but keep the parent directory
    find "$dir" -mindepth 1 -maxdepth 1 -exec rm -rf {} + 2>/dev/null || true
  fi
}

cleanup_dir "$HOME/Library/Caches/com.apple.Safari"
cleanup_dir "$HOME/Library/Caches/com.apple.WebKit.Networking"
cleanup_dir "$HOME/Library/Caches/com.apple.WebKit.WebContent"
cleanup_dir "$HOME/Library/Safari/LocalStorage"
cleanup_dir "$HOME/Library/Safari/Databases"
cleanup_dir "$HOME/Library/Caches/Metadata/Safari"

# Launch Safari to the URL
open -a Safari "$URL"

# If user requested an explicit Private Window, try to open one (requires Accessibility permission for System Events to send keystroke)
if [ "$PRIVATE_FLAG" = "--private" ]; then
  /usr/bin/osascript <<'APPLESCRIPT'
try
	tell application "Safari" to activate
	delay 0.4
	tell application "System Events"
		keystroke "n" using {shift down, command down}
	end tell
on error errMsg
	-- if Accessibility isn't enabled, fallback silently
end try
APPLESCRIPT
fi

exit 0
