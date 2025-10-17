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
    registerThemeListeners();
    updateThemeToggleIcon();
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

export default exported;
