const RECENT_STORAGE_KEY = 'nexacrm-recent-navigation';

// Debug logging to help when console messages seem absent. Use console.log
// (not console.debug) to avoid being filtered in some dev setups.
try { console.log('[layout.interop] module loaded'); } catch (e) { }

function resolveThemePreference() {
    const explicit = document.documentElement.getAttribute('data-theme')
        || localStorage.getItem('nexacrm-theme-preference')
        || 'light';

    if (explicit === 'auto' && window.matchMedia) {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    return explicit;
}

function updateThemeToggleIcon() {
    const themeToggle = document.querySelector('.theme-toggle-button') || document.querySelector('[data-theme-toggle]');
    if (!themeToggle) return;
    const lightIcon = themeToggle.querySelector('.theme-light-icon');
    const darkIcon = themeToggle.querySelector('.theme-dark-icon');
    if (!lightIcon || !darkIcon) return;
    const effectiveTheme = resolveThemePreference();
    try { console.log('[layout.interop] updateThemeToggleIcon effectiveTheme=', effectiveTheme); } catch (e) { }
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

function dedupeSidebar() {
    const nodes = Array.from(document.querySelectorAll('.sidebar'));
    try {
        if (!nodes || nodes.length <= 1) {
            try { console.log('[layout.interop] dedupeSidebar found', nodes.length, 'sidebar node(s)'); } catch(e){}
            return;
        }
        try { console.log('[layout.interop] dedupeSidebar found', nodes.length, 'sidebar nodes; removing extras'); } catch(e){}
        for (let i = 1; i < nodes.length; i++) {
            const node = nodes[i];
            if (node && node.parentNode) node.parentNode.removeChild(node);
        }
    } catch (e) {
        try { console.log('[layout.interop] dedupeSidebar encountered error', e); } catch (ee) {}
    }
    const toggles = Array.from(document.querySelectorAll('.btn-toggle-collapse'));
    if (toggles.length > 1) {
        try { console.log('[layout.interop] dedupeSidebar removing', toggles.length - 1, 'extra toggle(s)'); } catch(e){}
        for (let i = 1; i < toggles.length; i++) {
            const t = toggles[i];
            if (t && t.parentNode) t.parentNode.removeChild(t);
        }
    }
}

export function initializeShell() {
    try { console.log('[layout.interop] initializeShell start'); } catch(e){}
    try { dedupeSidebar(); } catch (e) { try { console.log('[layout.interop] dedupeSidebar failed', e); } catch(e2){} }
    try { updateThemeToggleIcon(); } catch (e) { try { console.log('[layout.interop] updateThemeToggleIcon failed', e); } catch(e2){} }
    try { console.log('[layout.interop] initializeShell end'); } catch(e){}
}

export function refreshThemeToggle() {
    try { console.log('[layout.interop] refreshThemeToggle'); } catch(e){}
    try { updateThemeToggleIcon(); } catch (e) { try { console.log('[layout.interop] refreshThemeToggle failed', e); } catch(e2){} }
}

export function getRecentNavigation() {
    try { return window.localStorage.getItem(RECENT_STORAGE_KEY); } catch { return null; }
}

export function saveRecentNavigation(json) {
    try { if (typeof json === 'string') window.localStorage.setItem(RECENT_STORAGE_KEY, json); } catch {}
}

export function clearRecentNavigation() {
    try { window.localStorage.removeItem(RECENT_STORAGE_KEY); } catch {}
}

export function focusGlobalSearch() {
    const input = document.querySelector('[data-global-search]');
    if (input instanceof HTMLElement) input.focus();
}

const exported = {
    initializeShell,
    refreshThemeToggle,
    getRecentNavigation,
    saveRecentNavigation,
    clearRecentNavigation,
    focusGlobalSearch
};

if (!window.layoutInterop) window.layoutInterop = exported; else Object.assign(window.layoutInterop, exported);

// also expose globals for non-module fallback
try {
    window.initializeShell = initializeShell;
    window.refreshThemeToggle = refreshThemeToggle;
    window.getRecentNavigation = getRecentNavigation;
    window.saveRecentNavigation = saveRecentNavigation;
    window.clearRecentNavigation = clearRecentNavigation;
    window.focusGlobalSearch = focusGlobalSearch;
} catch (e) { /* ignore */ }

export default exported;
