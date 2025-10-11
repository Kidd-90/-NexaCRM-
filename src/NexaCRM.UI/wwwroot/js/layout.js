const RECENT_STORAGE_KEY = 'nexacrm-recent-navigation';
let listenersRegistered = false;

function resolveThemePreference() {
    const explicit = document.documentElement.getAttribute('data-theme')
        || localStorage.getItem('nexacrm-theme-preference')
        || 'light';

    if (explicit === 'auto' && window.matchMedia) {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    return explicit;
}

function queryThemeToggle() {
    return document.querySelector('.theme-toggle-button');
}

function updateThemeToggleIcon() {
    const themeToggle = queryThemeToggle();
    if (!themeToggle) {
        return;
    }

    const lightIcon = themeToggle.querySelector('.theme-light-icon');
    const darkIcon = themeToggle.querySelector('.theme-dark-icon');
    if (!lightIcon || !darkIcon) {
        return;
    }

    const effectiveTheme = resolveThemePreference();
    if (effectiveTheme === 'dark') {
        lightIcon.style.display = 'inline';
        darkIcon.style.display = 'none';
        themeToggle.setAttribute('aria-label', 'Switch to light theme');
    } else {
        lightIcon.style.display = 'none';
        darkIcon.style.display = 'inline';
        themeToggle.setAttribute('aria-label', 'Switch to dark theme');
    }
}

function registerThemeListeners() {
    if (listenersRegistered) {
        return;
    }

    listenersRegistered = true;

    if (window.themeManager?.setupThemeToggleListeners) {
        window.themeManager.setupThemeToggleListeners();
    }

    document.addEventListener('DOMContentLoaded', () => {
        window.setTimeout(updateThemeToggleIcon, 150);
    });

    window.addEventListener('pageshow', () => {
        window.setTimeout(updateThemeToggleIcon, 120);
    });

    window.addEventListener('themeChanged', () => {
        window.setTimeout(updateThemeToggleIcon, 50);
    });

    if (window.matchMedia) {
        const media = window.matchMedia('(prefers-color-scheme: dark)');
        media.addEventListener('change', () => {
            const currentTheme = localStorage.getItem('nexacrm-theme-preference') || 'auto';
            if (currentTheme === 'auto') {
                window.setTimeout(updateThemeToggleIcon, 80);
            }
        });
    }
}

function setupNavigationInteractions(options) {
    const isMobile = Boolean(options?.isMobile);
    if (window.navigationHelper?.setupOverlayHandler) {
        window.navigationHelper.setupOverlayHandler();
    }

    if (window.navigationHelper?.toggleMenu) {
        window.navigationHelper.toggleMenu(isMobile);
    }

    if (isMobile && window.navigationHelper?.syncMobileLayoutSpacing) {
        window.navigationHelper.syncMobileLayoutSpacing();
    }
}

export function initializeShell(options) {
    setupNavigationInteractions(options);
    registerThemeListeners();
    updateThemeToggleIcon();
}

export function toggleMenu(forceClose) {
    if (window.navigationHelper?.toggleMenu) {
        window.navigationHelper.toggleMenu(forceClose);
    }
}

export function syncMobileLayout() {
    if (window.navigationHelper?.syncMobileLayoutSpacing) {
        window.navigationHelper.syncMobileLayoutSpacing();
    }
}

export function refreshThemeToggle() {
    updateThemeToggleIcon();
}

export function getRecentNavigation() {
    try {
        return window.localStorage.getItem(RECENT_STORAGE_KEY);
    } catch {
        return null;
    }
}

export function saveRecentNavigation(json) {
    try {
        if (typeof json === 'string') {
            window.localStorage.setItem(RECENT_STORAGE_KEY, json);
        }
    } catch {
        // ignore persistence failures
    }
}

export function clearRecentNavigation() {
    try {
        window.localStorage.removeItem(RECENT_STORAGE_KEY);
    } catch {
        // ignore persistence failures
    }
}

export function focusGlobalSearch() {
    const input = document.querySelector('[data-global-search]');
    if (input instanceof HTMLElement) {
        input.focus();
    }
}

// --- header measurement helpers ---
let __measureTimeout = null;
// Desired vertical gap (in px) between the bottom of the header and the
// start of the content. The page content should appear within this gap
// below the header (user requirement is 10px).
const DESIRED_CONTENT_GAP_PX = 10;
// Default assumed header height when measurement isn't available yet. This
// is used as a safe fallback during initial load. Match common header size.
const DEFAULT_HEADER_HEIGHT_PX = 37;
// Stabilization: avoid tiny fluctuations in header measurement from repeatedly
// updating the CSS variable. We keep the last applied offset and ignore
// changes smaller than the threshold. If a large layout change occurs it will
// still update.
let __lastAppliedOffset = null;
const HEADER_OFFSET_CHANGE_THRESHOLD_PX = 4;
// Expose a global flag that indicates when layout measurement and offsets
// have stabilized. Tests will wait for this flag to be true before
// performing assertions. We set it to true after the first successful
// application of content offsets and header measurement.
let __layoutStable = false;
// Debug logging: enable by adding `?layoutDebug=1` to the URL or
// setting `localStorage.setItem('nexacrm-layout-debug','1')` in the console.
const LAYOUT_DEBUG = (typeof window !== 'undefined') && (function () {
    try {
        const s = typeof window !== 'undefined' && window.location ? window.location.search : '';
        const params = new URLSearchParams(s || '');
        if (params.has('layoutDebug')) return true;
        return window.localStorage && window.localStorage.getItem('nexacrm-layout-debug') === '1';
    } catch (e) {
        return false;
    }
})();
let __headerObserver = null;
// Detect whether current layout is mobile. Prefer CSS class set by the app shell,
// fall back to viewport media query.
function isMobileLayout() {
    try {
        if (document.querySelector('.app-shell--mobile')) return true;
        // Respect explicit UA hint from user: treat common mobile UAs as mobile
        const ua = (typeof navigator !== 'undefined' && navigator.userAgent) ? navigator.userAgent : '';
        const MOBILE_UA = /(Mobi|Android|iPhone|iPad|iPod|Windows Phone|IEMobile|BlackBerry|BB10|webOS|Opera Mini|Opera Mobi|Kindle|Silk)/i;
        if (MOBILE_UA.test(ua)) return true;
        // iPadOS Safari sometimes reports as Mac; detect touch-capable Mac as iPad
        try {
            const platform = navigator.platform || '';
            if (/Mac/i.test(platform) && 'ontouchend' in document) return true;
        } catch { }
        return typeof window !== 'undefined' && window.matchMedia && window.matchMedia('(max-width: 767px)').matches;
    } catch {
        return false;
    }
}
function measureHeaderOffset() {
    try {
        if (__measureTimeout) {
            clearTimeout(__measureTimeout);
        }
        __measureTimeout = window.setTimeout(() => {
            __measureTimeout = null;
            const header = document.querySelector('.app-shell__page-header');
            const mobile = isMobileLayout();
            // If header is not yet present, fall back to a conservative default
            // header height and compute the effective offset as height + gap.
            // On desktop (non-mobile) the page header is hidden by design, so we force 0 offset.
            const headerComputed = header ? getComputedStyle(header) : null;
            const headerHidden = !header || headerComputed.display === 'none' || headerComputed.visibility === 'hidden';
            if (!header || headerHidden) {
                // Apply 67px on desktop and mobile; add default gap on mobile
                const effectiveDefault = mobile ? Math.max(0, DEFAULT_HEADER_HEIGHT_PX + DESIRED_CONTENT_GAP_PX) : 67;
                // If we've already applied an offset and the default wouldn't
                // change it by more than the threshold, keep the existing value
                if (__lastAppliedOffset !== null && Math.abs(__lastAppliedOffset - effectiveDefault) <= HEADER_OFFSET_CHANGE_THRESHOLD_PX) {
                    try { applyContentOffsets(); } catch { }
                    return;
                }

                document.documentElement.style.setProperty('--app-shell-page-header-offset', effectiveDefault + 'px');
                document.documentElement.style.setProperty('--app-shell-page-header-offset-desktop', effectiveDefault + 'px');
                document.documentElement.style.setProperty('--app-shell-page-header-offset-mobile', effectiveDefault + 'px');
                // Also expose a header height variable (used to size the surface)
                document.documentElement.style.setProperty('--app-shell-page-header-height', (mobile ? DEFAULT_HEADER_HEIGHT_PX : 0) + 'px');
                __lastAppliedOffset = effectiveDefault;
                if (LAYOUT_DEBUG) console.info('[layout.debug] header missing, applying default offset=', effectiveDefault, 'headerHeight=', DEFAULT_HEADER_HEIGHT_PX);
                try { applyContentOffsets(); } catch { }
                return;
            }

            // Use the measured bounding rect height. rect.height includes
            // padding and border which visually occupy space — that's desired.
            // To reduce per-page jitter we take 3 quick samples and use the
            // median. If samples vary widely we schedule a re-measure after
            // a short delay so late DOM mutations can settle.
            const rect = header.getBoundingClientRect();
            const s1 = Math.ceil(rect.height);
            const s2 = Math.ceil(header.getBoundingClientRect().height);
            const s3 = Math.ceil(header.getBoundingClientRect().height);
            const samples = [s1, s2, s3].sort((a, b) => a - b);
            const height = samples[1]; // median
            const spread = samples[2] - samples[0];
            if (LAYOUT_DEBUG) console.info('[layout.debug] height samples', samples, 'median=', height, 'spread=', spread);
            // If samples disagree by a lot, give the page a moment and re-run
            // measurement to let any late layout updates complete.
            if (spread > (HEADER_OFFSET_CHANGE_THRESHOLD_PX * 3)) {
                if (LAYOUT_DEBUG) console.info('[layout.debug] sample spread high, scheduling re-measure', spread);
                // schedule another measurement after a short delay and skip
                // applying the offset now — the later measurement will update.
                window.setTimeout(() => { try { measureHeaderOffset(); } catch { } }, 60);
                return;
            }
            // expose the measured header height so other layout pieces
            // (like .app-shell__surface) can size relative to it
            document.documentElement.style.setProperty('--app-shell-page-header-height', height + 'px');
            // Compute the effective offset as header height + desired gap so
            // content starts within DESIRED_CONTENT_GAP_PX below the header.
            // On mobile, keep a small gap below the header. On desktop, use 67px.
            const effective = mobile ? Math.max(0, height + DESIRED_CONTENT_GAP_PX) : 67;

            // Stabilize offset updates: if we've already applied an offset and
            // the new effective value differs by less than the threshold, skip
            // updating to avoid jitter. Large changes still replace the value.
            if (__lastAppliedOffset !== null && Math.abs(__lastAppliedOffset - effective) <= HEADER_OFFSET_CHANGE_THRESHOLD_PX) {
                // still ensure content offsets are applied, but don't rewrite the CSS vars
                if (LAYOUT_DEBUG) console.info('[layout.debug] measured offset change small, skipping update', { last: __lastAppliedOffset, measured: effective, threshold: HEADER_OFFSET_CHANGE_THRESHOLD_PX });
                try { applyContentOffsets(); } catch { }
                return;
            }

            document.documentElement.style.setProperty('--app-shell-page-header-offset', effective + 'px');
            document.documentElement.style.setProperty('--app-shell-page-header-offset-desktop', effective + 'px');
            document.documentElement.style.setProperty('--app-shell-page-header-offset-mobile', effective + 'px');
            __lastAppliedOffset = effective;
            if (LAYOUT_DEBUG) console.info('[layout.debug] applied header offset', { effective, height, rectHeight: rect.height, mobile });
            // Mark layout as stable after we've applied a measured offset.
            // Tests can poll window.__nexacrm_layout_stable or listen for
            // a console message to detect readiness.
            if (!__layoutStable) {
                __layoutStable = true;
                try {
                    window.__nexacrm_layout_stable = true;
                } catch (e) { }
                try { console.info('[layout.ready] layout measurements stabilized'); } catch (e) { }
            }
            // Ensure content offsets (left margin / max-width) are applied after header measurement
            try { applyContentOffsets(); } catch { }
        }, 17);
    }
    catch (e) {
        // swallow measurement errors
    }
}

// Observe the header for runtime changes (childList/attributes) and re-run
// measurement when it changes. This helps pages that mutate header content
// after initial load (e.g. toolbars, user-info, responsive changes).
function ensureHeaderObserver() {
    try {
        const header = document.querySelector('.app-shell__page-header');
        if (!header) return;
        if (__headerObserver) return;
        __headerObserver = new MutationObserver(() => {
            try { measureHeaderOffset(); } catch { }
        });
        __headerObserver.observe(header, { attributes: true, childList: true, subtree: true });
    } catch { }
}

// Measure navigation rail width and apply inline offsets to the main content
// Apply offsets with small retries and a MutationObserver so late-rendered DOM
// still receives the correct inline styles. Inline styles should always win
// over scoped or page-level CSS rules.
let __applyRetries = 0;
let __applyObserver = null;
function applyContentOffsets() {
    try {
        const docEl = document.documentElement;
        const nav = document.querySelector('.nav-rail');
        const content = document.querySelector('.app-shell__content');

        // If the content element isn't present yet, retry a few times
        if (!content) {
            if (__applyRetries < 5) {
                __applyRetries++;
                window.setTimeout(applyContentOffsets, 120);
            }
            return;
        }

        // Determine sidebar width: prefer live measurement of the rail element
        let sidebarWidth = 0;
        if (nav instanceof HTMLElement) {
            const rect = nav.getBoundingClientRect();
            sidebarWidth = Math.ceil(rect.width) || 0;
        }

        // If measurement failed, fall back to the CSS variable used by styles
        if (!sidebarWidth) {
            const computed = getComputedStyle(docEl);
            const parsed = parseInt(computed.getPropertyValue('--sidebar-width'));
            if (!isNaN(parsed)) sidebarWidth = parsed;
        }

        // Add the same small gutter used in CSS (6px) so surface doesn't touch the rail
        const gutter = 6;
        const left = (sidebarWidth || 0) + gutter;

        // Apply inline styles which take precedence over stylesheet rules and scoped CSS
        content.style.marginLeft = left + 'px';
        content.style.maxWidth = `calc(100% - (${left}px))`;
        content.style.boxSizing = 'border-box';

        // If we successfully applied offsets, disconnect the observer (if any)
        if (__applyObserver) {
            try { __applyObserver.disconnect(); } catch { }
            __applyObserver = null;
        }
    }
    catch (e) {
        // ignore failures to avoid breaking layout JS
    }

    // If we still haven't found the nav or measured width, ensure a MutationObserver
    // is watching for DOM changes and trigger a retry when the DOM mutates.
    try {
        if (!__applyObserver && __applyRetries < 5) {
            __applyObserver = new MutationObserver(() => {
                try { applyContentOffsets(); } catch { }
            });
            __applyObserver.observe(document.documentElement || document.body, { childList: true, subtree: true });

            // Stop observing after a short period to avoid keeping the observer alive
            window.setTimeout(() => {
                try { __applyObserver && __applyObserver.disconnect(); } catch { }
                __applyObserver = null;
            }, 2000);
        }
    } catch { }
}

// Provide compatibility for components that still expect window-level helpers.
if (!window.layoutInterop) {
    window.layoutInterop = {
        initializeShell,
        toggleMenu,
        syncMobileLayout,
        refreshThemeToggle,
        getRecentNavigation,
        saveRecentNavigation,
        clearRecentNavigation,
        focusGlobalSearch,
        measureHeaderOffset,
        applyContentOffsets,
        // expose a quick-read property that tests can use
        isLayoutStable: () => Boolean(window.__nexacrm_layout_stable || __layoutStable)
    };
} else {
    // ensure measureHeaderOffset is available on the existing helper
    window.layoutInterop.measureHeaderOffset = measureHeaderOffset;
    // expose applyContentOffsets as well so callers (or console debugging) can force re-run
    window.layoutInterop.applyContentOffsets = applyContentOffsets;
    window.layoutInterop.isLayoutStable = () => Boolean(window.__nexacrm_layout_stable || __layoutStable);
}

// ensure we measure on page show and resize so offsets remain accurate
window.addEventListener('pageshow', () => { try { measureHeaderOffset(); } catch { } });
window.addEventListener('resize', () => { try { measureHeaderOffset(); } catch { } });
// ensure we run an initial measurement and apply offsets once DOM is ready
window.addEventListener('DOMContentLoaded', () => {
    try {
        // Set a safe default up-front: 0 on desktop (header hidden), mobile uses header height + gap
        const mobile = isMobileLayout();
        const effectiveDefault = mobile ? Math.max(0, DEFAULT_HEADER_HEIGHT_PX + DESIRED_CONTENT_GAP_PX) : 0;
        document.documentElement.style.setProperty('--app-shell-page-header-offset', effectiveDefault + 'px');
        document.documentElement.style.setProperty('--app-shell-page-header-offset-desktop', effectiveDefault + 'px');
        document.documentElement.style.setProperty('--app-shell-page-header-offset-mobile', effectiveDefault + 'px');
        // also expose a default header height so surface sizing remains predictable
        document.documentElement.style.setProperty('--app-shell-page-header-height', (mobile ? DEFAULT_HEADER_HEIGHT_PX : 0) + 'px');
        // If we only apply defaults (no measured header), consider layout stable
        // after initial DOMContentLoaded to avoid tests hanging waiting for
        // a measurement that will never occur on minimal pages.
        try {
            __layoutStable = true;
            window.__nexacrm_layout_stable = true;
        } catch (e) { }
        // run measurement and apply content offsets shortly after DOMContentLoaded
        window.setTimeout(() => { try { measureHeaderOffset(); applyContentOffsets(); } catch { } }, 120);
    } catch { }
});

export default {
    initializeShell,
    toggleMenu,
    syncMobileLayout,
    refreshThemeToggle,
    measureHeaderOffset,
    applyContentOffsets,
    getRecentNavigation,
    saveRecentNavigation,
    clearRecentNavigation,
    focusGlobalSearch
};
