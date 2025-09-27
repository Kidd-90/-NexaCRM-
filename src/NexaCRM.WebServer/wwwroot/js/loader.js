(function () {
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

    showLoader();
})();
