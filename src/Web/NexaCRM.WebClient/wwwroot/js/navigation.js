// Navigation menu functionality
window.navigationHelper = {
    // Navigation scroll and keyboard functionality
    setupNavigationScrolling: () => {
        const navContainer = document.querySelector('.nav-scroll-container');
        if (!navContainer) return;
        
        // Setup keyboard navigation
        window.navigationHelper.setupKeyboardNavigation(navContainer);
        
        // Setup scroll position indicators
        window.navigationHelper.setupScrollIndicators(navContainer);
        
        // Setup smooth scrolling for menu items
        window.navigationHelper.setupMenuItemScrolling(navContainer);
    },
    
    // Enhanced keyboard navigation
    setupKeyboardNavigation: (container) => {
        const navItems = container.querySelectorAll('.nav-link, .nav-section-header');
        let currentIndex = -1;
        
        // Focus management
        const focusItem = (index) => {
            navItems.forEach((item, i) => {
                item.classList.toggle('keyboard-focused', i === index);
            });
            
            if (index >= 0 && index < navItems.length) {
                const item = navItems[index];
                item.scrollIntoView({
                    behavior: 'smooth',
                    block: 'nearest',
                    inline: 'nearest'
                });
                
                // Set focus for accessibility
                item.focus();
            }
        };
        
        // Global keyboard event listener
        document.addEventListener('keydown', (e) => {
            // Only handle navigation when menu is open
            const sidebar = document.querySelector('.sidebar');
            if (!sidebar || sidebar.classList.contains('collapse')) return;
            
            switch (e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    currentIndex = Math.min(currentIndex + 1, navItems.length - 1);
                    focusItem(currentIndex);
                    break;
                    
                case 'ArrowUp':
                    e.preventDefault();
                    currentIndex = Math.max(currentIndex - 1, 0);
                    focusItem(currentIndex);
                    break;
                    
                case 'Enter':
                case ' ':
                    e.preventDefault();
                    if (currentIndex >= 0 && currentIndex < navItems.length) {
                        navItems[currentIndex].click();
                    }
                    break;
                    
                case 'Escape':
                    e.preventDefault();
                    // Close menu
                    window.navigationHelper.toggleMenu(true);
                    break;
                    
                case 'Home':
                    e.preventDefault();
                    currentIndex = 0;
                    focusItem(currentIndex);
                    break;
                    
                case 'End':
                    e.preventDefault();
                    currentIndex = navItems.length - 1;
                    focusItem(currentIndex);
                    break;
            }
        });
    },
    
    // Scroll position indicators
    setupScrollIndicators: (container) => {
        let scrollTimeout;
        
        container.addEventListener('scroll', () => {
            // Add scrolling class for visual feedback
            container.classList.add('scrolling');
            
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => {
                container.classList.remove('scrolling');
            }, 150);
            
            // Update scroll indicators
            const scrollTop = container.scrollTop;
            const scrollHeight = container.scrollHeight;
            const clientHeight = container.clientHeight;
            const scrollPercent = scrollTop / (scrollHeight - clientHeight);
            
            // Add data attribute for CSS styling
            container.setAttribute('data-scroll-percent', Math.round(scrollPercent * 100));
        });
    },
    
    // Enhanced menu item scrolling
    setupMenuItemScrolling: (container) => {
        const navItems = container.querySelectorAll('.nav-item');
        
        navItems.forEach(item => {
            const header = item.querySelector('.nav-section-header');
            if (header) {
                header.addEventListener('click', () => {
                    // Smooth scroll to ensure expanded submenu is visible
                    setTimeout(() => {
                        const submenu = item.querySelector('.nav-submenu');
                        if (submenu && submenu.offsetHeight > 0) {
                            const itemBottom = item.offsetTop + item.offsetHeight;
                            const containerBottom = container.scrollTop + container.clientHeight;
                            
                            if (itemBottom > containerBottom) {
                                container.scrollTo({
                                    top: item.offsetTop,
                                    behavior: 'smooth'
                                });
                            }
                        }
                    }, 100);
                });
            }
        });
    },

    // 오버레이 클릭 시 메뉴 닫기
    setupOverlayHandler: () => {
        const overlay = document.querySelector('.mobile-overlay');
        const sidebar = document.querySelector('.sidebar');

        if (overlay && sidebar) {
            overlay.addEventListener('click', (e) => {
                // Prevent event bubbling
                e.stopPropagation();
                // 공용 토글 함수 사용하여 메뉴 닫기
                window.navigationHelper.toggleMenu(true);
            });

            // Prevent scrolling when overlay is visible
            overlay.addEventListener('touchmove', (e) => {
                e.preventDefault();
            }, { passive: false });
        }
        
        // 초기 상태에서 메뉴를 확실히 숨김
        if (sidebar && !sidebar.classList.contains('collapse')) {
            sidebar.classList.add('collapse');
        }
        
        // Ensure overlay is hidden initially
        if (overlay) {
            overlay.classList.remove('show');
        }

        // Setup navigation scrolling functionality
        window.navigationHelper.setupNavigationScrolling();
    },

    // Keep mobile content offset from fixed header/footer heights
    syncMobileLayoutSpacing: () => {
        const root = document.documentElement;
        if (!root) return;

        const state = window.navigationHelper._mobileSpacingState || (window.navigationHelper._mobileSpacingState = {
            lastHeader: 72,
            lastFooter: 80,
            headerObserver: null,
            footerObserver: null,
            viewportHandler: null
        });

        const header = document.querySelector('.mobile-layout .mobile-fixed-header');
        const footer = document.querySelector('.mobile-layout .mobile-fixed-footer');

        const updateSpacing = () => {
            const headerRect = header ? header.getBoundingClientRect() : null;
            const footerRect = footer ? footer.getBoundingClientRect() : null;

            const headerHeight = Math.max(1, Math.ceil((headerRect && headerRect.height) || state.lastHeader || 72));
            const footerHeight = Math.max(1, Math.ceil((footerRect && footerRect.height) || state.lastFooter || 80));

            state.lastHeader = headerHeight;
            state.lastFooter = footerHeight;

            root.style.setProperty('--mobile-header-height', `${headerHeight}px`);
            root.style.setProperty('--mobile-footer-height', `${footerHeight}px`);
        };

        updateSpacing();

        if (state.headerObserver) {
            state.headerObserver.disconnect();
            state.headerObserver = null;
        }

        if (state.footerObserver) {
            state.footerObserver.disconnect();
            state.footerObserver = null;
        }

        if (typeof ResizeObserver !== 'undefined') {
            if (header) {
                state.headerObserver = new ResizeObserver(updateSpacing);
                state.headerObserver.observe(header);
            }

            if (footer) {
                state.footerObserver = new ResizeObserver(updateSpacing);
                state.footerObserver.observe(footer);
            }
        }

        if (state.viewportHandler) {
            window.removeEventListener('resize', state.viewportHandler);
            window.removeEventListener('orientationchange', state.viewportHandler);
        }

        state.viewportHandler = () => window.requestAnimationFrame(updateSpacing);

        window.addEventListener('resize', state.viewportHandler);
        window.addEventListener('orientationchange', state.viewportHandler);

        requestAnimationFrame(updateSpacing);
    },

    // Mobile dashboard navigation functionality
    setupMobileDashboardNavigation: () => {
        // Setup smooth scrolling for dashboard navigation links
        window.navigationHelper.setupSmoothScrolling();
        
        // Setup touch-friendly interactions for dashboard elements
        window.navigationHelper.setupTouchInteractions();
        
        // Setup click handlers for dashboard buttons and cards
        window.navigationHelper.setupDashboardClickHandlers();
        
        // Setup mobile responsiveness for dashboard
        window.navigationHelper.setupMobileResponsiveness();
    },

    // Smooth scrolling functionality
    setupSmoothScrolling: () => {
        // Enable smooth scrolling for all internal links
        document.addEventListener('click', (e) => {
            const link = e.target.closest('a[href^="#"]');
            if (link) {
                e.preventDefault();
                const targetId = link.getAttribute('href').substring(1);
                const targetElement = document.getElementById(targetId);
                
                if (targetElement) {
                    targetElement.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });

        // Smooth scrolling for dashboard sections
        const dashboardCards = document.querySelectorAll('.dashboard-card');
        dashboardCards.forEach(card => {
            card.addEventListener('click', (e) => {
                // Add smooth transition effect
                card.style.transform = 'scale(0.98)';
                setTimeout(() => {
                    card.style.transform = 'scale(1)';
                }, 150);
            });
        });
    },

    // Touch-friendly interactions
    setupTouchInteractions: () => {
        // Add touch feedback for interactive elements
        const interactiveElements = document.querySelectorAll(
            'button, .nav-link, .dashboard-card, .dashboard-button, .submenu-link, a[href]'
        );

        interactiveElements.forEach(element => {
            // Add touch start feedback
            element.addEventListener('touchstart', (e) => {
                element.style.opacity = '0.85';
                element.style.transform = 'scale(0.98)';
            }, { passive: true });

            // Remove touch feedback
            element.addEventListener('touchend', (e) => {
                setTimeout(() => {
                    element.style.opacity = '';
                    element.style.transform = '';
                }, 100);
            }, { passive: true });

            // Handle touch cancel
            element.addEventListener('touchcancel', (e) => {
                element.style.opacity = '';
                element.style.transform = '';
            }, { passive: true });
        });

        // Improve scrolling performance on touch devices
        const scrollableElements = document.querySelectorAll('.sidebar, .dashboard-main-content, main');
        scrollableElements.forEach(element => {
            element.style.webkitOverflowScrolling = 'touch';
            element.style.overflowScrolling = 'touch';
        });
    },

    // Dashboard click handlers
    setupDashboardClickHandlers: () => {
        // Handle dashboard card clicks
        const dashboardCards = document.querySelectorAll('.dashboard-card');
        dashboardCards.forEach(card => {
            card.addEventListener('click', (e) => {
                const route = card.dataset.route;
                if (!route) return;

                console.log(`Dashboard card clicked, route: ${route}`);
                
                // Add visual feedback
                card.classList.add('clicked');
                setTimeout(() => {
                    card.classList.remove('clicked');
                }, 300);

                if (route.startsWith('#')) {
                    // Handle smooth scrolling to section
                    const sectionId = route.substring(1);
                    const element = document.getElementById(sectionId);
                    if (element) {
                        element.scrollIntoView({ behavior: 'smooth' });
                    }
                } else {
                    // Navigate to new page
                    window.location.href = route;
                }
            });
        });

        // Handle dashboard button clicks
        const dashboardButtons = document.querySelectorAll('.dashboard-button');
        dashboardButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                e.stopPropagation();
                
                // Add click animation
                button.style.transform = 'scale(0.95)';
                setTimeout(() => {
                    button.style.transform = '';
                }, 100);

                // Handle button functionality
                console.log('Dashboard button clicked');
            });
        });

        // Handle sidebar navigation in dashboard
        const dashboardSidebarLinks = document.querySelectorAll('.dashboard-sidebar a');
        dashboardSidebarLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                // Close mobile navigation when dashboard sidebar link is clicked
                const sidebar = document.querySelector('.sidebar');
                const overlay = document.querySelector('.mobile-overlay');
                
                if (window.innerWidth <= 768) {
                    if (sidebar) sidebar.classList.add('collapse');
                    if (overlay) overlay.classList.remove('show');
                }
            });
        });
    },

    // Mobile responsiveness improvements
    setupMobileResponsiveness: () => {
        // Hide dashboard sidebar and top nav on mobile
        const updateMobileLayout = () => {
            const dashboardSidebar = document.querySelector('.dashboard-sidebar');
            const dashboardTopNav = document.querySelector('.dashboard-top-nav');
            const isMobile = window.innerWidth <= 768;

            if (dashboardSidebar) {
                dashboardSidebar.style.display = isMobile ? 'none' : '';
            }
            if (dashboardTopNav) {
                dashboardTopNav.style.display = isMobile ? 'none' : '';
            }

            // Adjust main content spacing on mobile
            const dashboardMainContent = document.querySelector('.dashboard-main-content');
            if (dashboardMainContent && isMobile) {
                dashboardMainContent.style.maxWidth = '100%';
                dashboardMainContent.style.padding = '1rem';
            }
        };

        // Initial setup
        updateMobileLayout();

        // Update on resize
        window.addEventListener('resize', updateMobileLayout);
        window.addEventListener('orientationchange', () => {
            setTimeout(updateMobileLayout, 100);
        });
    },

    // 자동 로그아웃 처리
    setupAutoLogout: () => {
        // 페이지 언로드 시 (브라우저 종료, 탭 닫기, 새로고침)
        window.addEventListener('beforeunload', () => {
            // 세션스토리지를 사용해서 브라우저 종료 구분
            sessionStorage.setItem('isUnloading', 'true');
            
            // setTimeout을 사용해서 새로고침인지 구분
            setTimeout(() => {
                sessionStorage.removeItem('isUnloading');
            }, 100);
        });

        // 페이지 로드 시 브라우저 종료 후 재시작인지 확인
        window.addEventListener('load', () => {
            const wasUnloading = sessionStorage.getItem('isUnloading');
            if (!wasUnloading) {
                // 브라우저가 완전히 종료되었다가 다시 시작된 경우
                // localStorage의 인증 정보 제거
                const username = localStorage.getItem('username');
                if (username) {
                    console.log('Browser was closed, clearing authentication data');
                    localStorage.removeItem('username');
                    localStorage.removeItem('roles');
                }
            }
            // cleanup
            sessionStorage.removeItem('isUnloading');
        });

        // 탭이 비활성화된 후 일정 시간이 지나면 자동 로그아웃 (30분)
        let lastActiveTime = Date.now();
        let timeoutId = null;

        const resetTimeout = () => {
            lastActiveTime = Date.now();
            if (timeoutId) clearTimeout(timeoutId);
            
            timeoutId = setTimeout(() => {
                const username = localStorage.getItem('username');
                if (username) {
                    console.log('Session timeout, clearing authentication data');
                    localStorage.removeItem('username');
                    localStorage.removeItem('roles');
                    window.location.href = '/login';
                }
            }, 30 * 60 * 1000); // 30분
        };

        // 사용자 활동 감지 이벤트들
        ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'].forEach(event => {
            document.addEventListener(event, resetTimeout, true);
        });

        // 페이지 포커스/블러 이벤트
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                // 탭이 비활성화됨
                console.log('Tab became inactive');
            } else {
                // 탭이 활성화됨
                console.log('Tab became active');
                resetTimeout();
            }
        });

        // 초기 타임아웃 설정
        resetTimeout();
    },

    // Page transition handler for smoother navigation
    setupPageTransitions: () => {
        const content = document.querySelector('article.content');
        if (!content) return;

        // Remove enter class after animation completes
        content.addEventListener('animationend', (e) => {
            if (e.animationName === 'fadeInUp') {
                content.classList.remove('page-fade-enter');
            }

            if (e.animationName === 'fadeOutDown') {
                const target = content.dataset.navigateTo;
                content.classList.remove('page-fade-exit');
                content.dataset.navigateTo = '';
                if (target) {
                    // Navigate after fade-out completes
                    Blazor.navigateTo(target);
                    requestAnimationFrame(() => {
                        content.classList.add('page-fade-enter');
                    });
                }
            }
        });

        // Intercept navigation link clicks
        document.querySelectorAll('.nav-link').forEach(link => {
            link.addEventListener('click', (e) => {
                const hrefValue = link.getAttribute('href');
                if (hrefValue === null || hrefValue.startsWith('#') || link.target === '_blank') return;

                const href = hrefValue === '' ? '/' : hrefValue;

                e.preventDefault();
                e.stopPropagation();

                content.dataset.navigateTo = href;
                content.classList.remove('page-fade-enter');
                content.classList.add('page-fade-exit');

                // Close the menu with animation
                window.navigationHelper.toggleMenu(true);
            }, { capture: true });
        });
    },

    // 컴포넌트에서 호출할 수 있는 메뉴 토글 함수
    toggleMenu: (isCollapsed) => {
        const sidebar = document.querySelector('.sidebar');
        const overlay = document.querySelector('.mobile-overlay');
        const toggleBtn = document.querySelector('.floating-menu-toggle');

        if (!sidebar) return;

        if (typeof isCollapsed === 'undefined') {
            isCollapsed = !sidebar.classList.contains('collapse');
        }

        if (isCollapsed) {
            sidebar.classList.add('collapse');
            if (overlay) {
                overlay.classList.remove('show');
            }
        } else {
            sidebar.classList.remove('collapse');
            if (overlay) {
                overlay.classList.add('show');
            }
        }

        if (toggleBtn) {
            toggleBtn.style.display = isCollapsed ? 'flex' : 'none';
        }

        return isCollapsed;
    },
    
    // 페이지 로드 시 초기화
    initialize: () => {
        // DOM이 준비되면 오버레이 핸들러 설정
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                window.navigationHelper.setupOverlayHandler();
                window.navigationHelper.setupAutoLogout();
                window.navigationHelper.setupMobileDashboardNavigation();
                window.navigationHelper.setupPageTransitions();
            });
        } else {
            window.navigationHelper.setupOverlayHandler();
            window.navigationHelper.setupAutoLogout();
            window.navigationHelper.setupMobileDashboardNavigation();
            window.navigationHelper.setupPageTransitions();
        }
        
        // 즉시 실행하여 초기 상태 보장
        setTimeout(() => {
            const sidebar = document.querySelector('.sidebar');
            const overlay = document.querySelector('.mobile-overlay');
            
            if (sidebar && !sidebar.classList.contains('collapse')) {
                sidebar.classList.add('collapse');
            }
            
            // Ensure overlay is hidden on page load
            if (overlay && overlay.classList.contains('show')) {
                overlay.classList.remove('show');
            }
        }, 100);

        // Re-setup dashboard navigation when page content changes (for SPA routing)
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                    // Check if dashboard content was added
                    const dashboardAdded = Array.from(mutation.addedNodes).some(node => 
                        node.nodeType === 1 && (
                            node.querySelector && (
                                node.querySelector('[data-page="main-dashboard"]') ||
                                node.classList && node.classList.contains('dashboard-card')
                            )
                        )
                    );
                    
                    if (dashboardAdded) {
                        setTimeout(() => {
                            window.navigationHelper.setupMobileDashboardNavigation();
                        }, 100);
                    }
                }
            });
        });

        // Observe changes to the main content area
        const mainElement = document.querySelector('main') || document.body;
        if (mainElement) {
            observer.observe(mainElement, {
                childList: true,
                subtree: true
            });
        }
    }
};

// 초기화 실행
window.navigationHelper.initialize();
