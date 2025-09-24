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
        focusGlobalSearch
    };
}

export default {
    initializeShell,
    toggleMenu,
    syncMobileLayout,
    refreshThemeToggle,
    getRecentNavigation,
    saveRecentNavigation,
    clearRecentNavigation,
    focusGlobalSearch
};
