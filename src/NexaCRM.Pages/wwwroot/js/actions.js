let fabOutsideHandlerRegistered = false;
let mobileDashboardInitialized = false;

function safelyDetach(node) {
    if (!node) {
        return;
    }

    const parent = node.parentNode;
    if (parent && typeof parent.removeChild === 'function') {
        try {
            parent.removeChild(node);
            return;
        } catch (err) {
            console.warn('Failed to detach node via parent.removeChild', err);
        }
    }

    if (typeof node.remove === 'function') {
        try {
            node.remove();
        } catch (err) {
            console.warn('Failed to detach node via node.remove()', err);
        }
    }
}

function ensureNavigator() {
    if (typeof navigator === 'undefined') {
        return {};
    }

    return navigator;
}

export function vibrate(duration) {
    const vib = ensureNavigator().vibrate;
    if (typeof vib === 'function') {
        try {
            vib.call(navigator, duration);
        } catch (err) {
            console.warn('Vibration failed', err);
        }
    }
}

export function openTel(phoneNumber) {
    if (!phoneNumber) {
        return;
    }

    try {
        window.location.href = `tel:${encodeURIComponent(phoneNumber)}`;
    } catch (err) {
        console.warn('Telephone deep link failed', err);
    }
}

export function openMailto(mailtoUrl) {
    if (!mailtoUrl) {
        return;
    }

    try {
        window.location.href = mailtoUrl;
    } catch (err) {
        console.warn('Mailto deep link failed', err);
    }
}

export async function copyText(value) {
    if (!value) {
        return false;
    }

    const nav = ensureNavigator();
    if (nav.clipboard && typeof nav.clipboard.writeText === 'function') {
        try {
            await nav.clipboard.writeText(value);
            return true;
        } catch (err) {
            console.warn('Clipboard write failed', err);
        }
    }

    try {
        const textarea = document.createElement('textarea');
        textarea.value = value;
        textarea.setAttribute('readonly', '');
        textarea.style.position = 'absolute';
        textarea.style.left = '-9999px';
        document.body.appendChild(textarea);
        textarea.select();
        const succeeded = document.execCommand('copy');
        safelyDetach(textarea);
        return succeeded;
    } catch (err) {
        console.warn('Fallback clipboard copy failed', err);
        return false;
    }
}

export function triggerDownload(base64Data, fileName, contentType) {
    if (!base64Data || !fileName) {
        return;
    }

    const mime = contentType || 'application/octet-stream';
    const link = document.createElement('a');
    link.href = `data:${mime};base64,${base64Data}`;
    link.download = fileName;
    link.rel = 'noopener';
    link.style.display = 'none';
    document.body.appendChild(link);
    link.click();
    safelyDetach(link);
}

export function smoothScrollToId(elementId) {
    if (!elementId) {
        return;
    }

    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

export function focusSelector(selector) {
    if (!selector) {
        return;
    }

    const element = document.querySelector(selector);
    if (element instanceof HTMLElement) {
        element.focus();
    }
}

export function isMobileViewport() {
    if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') {
        return false;
    }

    try {
        return window.matchMedia('(max-width: 767px)').matches;
    } catch (err) {
        console.warn('Failed to evaluate mobile viewport query', err);
        return false;
    }
}

export function registerFabOutsideHandler() {
    if (fabOutsideHandlerRegistered || typeof document === 'undefined') {
        return;
    }

    document.addEventListener('click', (event) => {
        const target = event.target;
        if (target instanceof Element && target.closest('.fab-container')) {
            return;
        }

        const expanded = document.querySelector('.fab-container.expanded .fab-main');
        if (expanded instanceof HTMLElement) {
            expanded.click();
        }
    });

    fabOutsideHandlerRegistered = true;
}

export function setupMobileDashboard() {
    if (typeof document === 'undefined') {
        return;
    }

    if (mobileDashboardInitialized) {
        return;
    }

    try {
        if (window.navigationHelper && typeof window.navigationHelper.setupMobileDashboardNavigation === 'function') {
            window.navigationHelper.setupMobileDashboardNavigation();
        }
    } catch (err) {
        console.warn('navigationHelper.setupMobileDashboardNavigation failed', err);
    }

    try {
        const mobileHeader = document.querySelector('[data-mobile-header]');
        if (mobileHeader instanceof HTMLElement) {
            mobileHeader.setAttribute('data-active', 'true');
        }

        const handleBackdropClick = (event) => {
            const target = event.target;
            if (!(target instanceof Element)) {
                return;
            }

            if (target.classList.contains('mobile-search-bar') && target.classList.contains('expanded')) {
                const closeSearch = document.querySelector('.mobile-search-close');
                if (closeSearch instanceof HTMLElement) {
                    closeSearch.click();
                }
            }

            if (target.classList.contains('mobile-notifications-panel') && target.classList.contains('expanded')) {
                const closeNotifications = document.querySelector('.mobile-notifications-close');
                if (closeNotifications instanceof HTMLElement) {
                    closeNotifications.click();
                }
            }
        };

        document.addEventListener('click', handleBackdropClick);

        let touchStartY = 0;
        document.addEventListener(
            'touchstart',
            (event) => {
                if (event.touches && event.touches[0]) {
                    touchStartY = event.touches[0].clientY;
                }
            },
            { passive: true }
        );

        document.addEventListener(
            'touchend',
            (event) => {
                if (!event.changedTouches || !event.changedTouches[0]) {
                    return;
                }

                const touchEndY = event.changedTouches[0].clientY;
                const diff = touchStartY - touchEndY;

                if (diff > 50) {
                    document.querySelectorAll('.mobile-search-bar.expanded, .mobile-notifications-panel.expanded').forEach((panel) => {
                        const closeBtn = panel.querySelector('.mobile-search-close, .mobile-notifications-close');
                        if (closeBtn instanceof HTMLElement) {
                            closeBtn.click();
                        }
                    });
                }
            },
            { passive: true }
        );
    } catch (err) {
        console.warn('setupMobileDashboard failed', err);
    }

    mobileDashboardInitialized = true;
}

export function setLocation(href) {
    if (!href) {
        return;
    }

    try {
        window.location.href = href;
    } catch (err) {
        console.warn('Failed to navigate to href', err);
    }
}

export default {
    vibrate,
    openTel,
    openMailto,
    copyText,
    triggerDownload,
    smoothScrollToId,
    focusSelector,
    isMobileViewport,
    registerFabOutsideHandler,
    setupMobileDashboard,
    setLocation
};
