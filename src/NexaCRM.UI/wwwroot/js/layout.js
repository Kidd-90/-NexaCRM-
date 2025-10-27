const RECENT_STORAGE_KEY = 'nexacrm-recent-navigation';
let listenersRegistered = false;
try { console.log('[layout] script loaded'); } catch (e) {}

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
    return document.querySelector('.theme-toggle-button')
        || document.querySelector('[data-theme-toggle]');
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
    try { console.log('[layout] updateThemeToggleIcon effectiveTheme=', effectiveTheme); } catch (e) {}
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

function initializeShell() {
    try { console.log('[layout] initializeShell start'); } catch (e) {}
    registerThemeListeners();
    // Ensure only a single sidebar/navigation rail exists in the document.
    // In some client/server render scenarios duplicate nodes may be present;
    // remove any extras to avoid invisible/disabled UI fragments.
    try {
        dedupeSidebar();
    } catch (e) {
        // swallow errors to avoid breaking initialize
        try { console.log('[layout] dedupeSidebar failed', e); } catch (ex) {}
    }
    try { updateThemeToggleIcon(); } catch (e) { try { console.log('[layout] updateThemeToggleIcon failed', e); } catch(ex){} }
    try { console.log('[layout] initializeShell end'); } catch (e) {}
}

function dedupeSidebar() {
    const nodes = Array.from(document.querySelectorAll('.sidebar'));
    if (!nodes || nodes.length <= 1) return;
    // Keep the first occurrence and remove subsequent duplicates
    for (let i = 1; i < nodes.length; i++) {
        const node = nodes[i];
        if (node && node.parentNode) {
            node.parentNode.removeChild(node);
        }
    }
    // Also remove any stray .btn-toggle-collapse duplicates
    const toggles = Array.from(document.querySelectorAll('.btn-toggle-collapse'));
    if (toggles.length > 1) {
        for (let i = 1; i < toggles.length; i++) {
            const t = toggles[i];
            if (t && t.parentNode) t.parentNode.removeChild(t);
        }
    }
}

function toggleMenu() {
    // Desktop 레이아웃에서는 별도 메뉴 토글이 필요하지 않으므로 항상 false 반환
    return false;
}

function refreshThemeToggle() {
    updateThemeToggleIcon();
}

function getRecentNavigation() {
    try {
        return window.localStorage.getItem(RECENT_STORAGE_KEY);
    } catch {
        return null;
    }
}

function saveRecentNavigation(json) {
    try {
        if (typeof json === 'string') {
            window.localStorage.setItem(RECENT_STORAGE_KEY, json);
        }
    } catch {
        // ignore persistence failures
    }
}

function clearRecentNavigation() {
    try {
        window.localStorage.removeItem(RECENT_STORAGE_KEY);
    } catch {
        // ignore persistence failures
    }
}

function focusGlobalSearch() {
    const input = document.querySelector('[data-global-search]');
    if (input instanceof HTMLElement) {
        input.focus();
    }
}

const exported = {
    initializeShell,
    toggleMenu,
    refreshThemeToggle,
    getRecentNavigation,
    saveRecentNavigation,
    clearRecentNavigation,
    focusGlobalSearch
};

if (!window.layoutInterop) {
    window.layoutInterop = exported;
} else {
    Object.assign(window.layoutInterop, exported);
}

// Expose named functions as globals to support non-module fallback calls
try {
    window.initializeShell = initializeShell;
    window.toggleMenu = toggleMenu;
    window.refreshThemeToggle = refreshThemeToggle;
    window.getRecentNavigation = getRecentNavigation;
    window.saveRecentNavigation = saveRecentNavigation;
    window.clearRecentNavigation = clearRecentNavigation;
    window.focusGlobalSearch = focusGlobalSearch;
} catch (e) {
    // ignore when window is not writable in some environments
}

// Also export named functions so ES module import() receives them directly.
export { initializeShell, toggleMenu, refreshThemeToggle, getRecentNavigation, saveRecentNavigation, clearRecentNavigation, focusGlobalSearch };

export default exported;
