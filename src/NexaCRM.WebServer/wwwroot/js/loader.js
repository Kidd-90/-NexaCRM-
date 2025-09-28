(function () {
    if (window.nexacrmLoader && window.nexacrmLoader._nexacrm_init) return;
    'use strict';

    const loaderId = 'nexacrm-initial-loader';
    let isVisible = true;

    function getLoader() {
        return document.getElementById(loaderId);
    }

    function hideLoader() {
        if (!isVisible) {
            return;
        }

        const loader = getLoader();
        if (!loader) {
            isVisible = false;
            return;
        }

        isVisible = false;
        loader.setAttribute('aria-hidden', 'true');
        loader.classList.add('is-hidden');
    }

    function showLoader() {
        const loader = getLoader();
        if (!loader) {
            isVisible = true;
            return;
        }

        isVisible = true;
        loader.classList.remove('is-hidden');
        loader.removeAttribute('aria-hidden');
    }

    window.addEventListener('blazor:connected', hideLoader);
    window.addEventListener('blazor:disconnected', showLoader);

    // Fallback: hide loader after 5 seconds if Blazor hasn't connected
    setTimeout(() => {
        if (isVisible) {
            console.warn('Blazor did not connect within 5 seconds, hiding loader.');
            hideLoader();
        }
    }, 5000);

    showLoader();
    try { window.nexacrmLoader = window.nexacrmLoader || {}; window.nexacrmLoader._nexacrm_init = true; } catch (e) { }
})();
