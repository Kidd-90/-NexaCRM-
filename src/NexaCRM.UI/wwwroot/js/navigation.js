(function () {
    if (window.navigationHelper && window.navigationHelper._nexacrm_init) {
        return;
    }

    const helper = {
        toggleMenu: () => false,
        setupOverlayHandler: () => { /* desktop shell no longer needs overlay handlers */ },
        syncMobileLayoutSpacing: () => { /* no-op in desktop layout */ },
        setupNavigationScrolling: () => { /* deprecated */ },
        setupMobileDashboardNavigation: () => { /* retained for backwards compatibility */ },

        setupAutoLogout: () => {
            // 페이지 언로드 시 (브라우저 종료, 탭 닫기, 새로고침)
            window.addEventListener('beforeunload', () => {
                sessionStorage.setItem('isUnloading', 'true');
                setTimeout(() => {
                    sessionStorage.removeItem('isUnloading');
                }, 100);
            });

            window.addEventListener('load', () => {
                const wasUnloading = sessionStorage.getItem('isUnloading');
                if (!wasUnloading) {
                    const username = localStorage.getItem('username');
                    if (username) {
                        localStorage.removeItem('username');
                        localStorage.removeItem('roles');
                        localStorage.removeItem('isDeveloper');
                    }
                }

                sessionStorage.removeItem('isUnloading');
            });

            let lastActiveTime = Date.now();
            let timeoutId = null;

            const resetTimeout = () => {
                lastActiveTime = Date.now();
                if (timeoutId) {
                    clearTimeout(timeoutId);
                }

                timeoutId = setTimeout(() => {
                    const username = localStorage.getItem('username');
                    if (username) {
                        localStorage.removeItem('username');
                        localStorage.removeItem('roles');
                        localStorage.removeItem('isDeveloper');
                        window.location.href = '/login';
                    }
                }, 30 * 60 * 1000);
            };

            ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'].forEach(event => {
                document.addEventListener(event, resetTimeout, true);
            });

            document.addEventListener('visibilitychange', () => {
                if (!document.hidden) {
                    resetTimeout();
                }
            });

            resetTimeout();
        },

        setupPageTransitions: () => {
            const content = document.querySelector('article.content');
            if (!content) {
                return;
            }

            content.addEventListener('animationend', (e) => {
                if (e.animationName === 'fadeInUp') {
                    content.classList.remove('page-fade-enter');
                }

                if (e.animationName === 'fadeOutDown') {
                    const target = content.dataset.navigateTo;
                    content.classList.remove('page-fade-exit');
                    content.dataset.navigateTo = '';
                    if (target) {
                        Blazor.navigateTo(target);
                        requestAnimationFrame(() => {
                            content.classList.add('page-fade-enter');
                        });
                    }
                }
            });

            document.querySelectorAll('a.nav-link').forEach(link => {
                link.addEventListener('click', (e) => {
                    const hrefValue = link.getAttribute('href');
                    if (hrefValue === null || hrefValue.startsWith('#') || link.target === '_blank') {
                        return;
                    }

                    const href = hrefValue === '' ? '/' : hrefValue;
                    e.preventDefault();
                    e.stopPropagation();

                    content.dataset.navigateTo = href;
                    content.classList.remove('page-fade-enter');
                    content.classList.add('page-fade-exit');
                }, { capture: true });
            });
        },

        initialize: () => {
            const initActions = () => {
                try { helper.setupAutoLogout(); } catch (err) { console.warn('auto logout init failed', err); }
                try { helper.setupPageTransitions(); } catch (err) { console.warn('page transitions init failed', err); }
            };

            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', initActions, { once: true });
            } else {
                initActions();
            }
        }
    };

    helper._nexacrm_init = true;
    window.navigationHelper = Object.assign(window.navigationHelper || {}, helper);

    helper.initialize();
})();
