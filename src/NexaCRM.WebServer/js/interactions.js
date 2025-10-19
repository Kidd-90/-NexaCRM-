// Enhanced Interactions and Micro-animations for NexaCRM
(function(){
    // Prevent double-initialization when the script is loaded more than once
    if (window.interactionManager && window.interactionManager._nexacrm_init) {
        console.debug('interactionManager already initialized; skipping re-init');
        return;
    }

    const safeRemoveNode = (node) => {
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
    };

    const _manager = {
        // Initialize all interaction enhancements
        init: () => {
            console.log('Initializing interaction enhancements...');

            // Setup enhanced touch interactions
            _manager.setupEnhancedTouchInteractions();

            // Setup ripple effects
            _manager.setupRippleEffects();

            // Setup swipe gestures
            _manager.setupSwipeGestures();

            // Setup pull-to-refresh
            _manager.setupPullToRefresh();

            // Setup intersection observer for animations
            _manager.setupScrollAnimations();

            // Setup enhanced keyboard navigation
            _manager.setupEnhancedKeyboardNavigation();

            // Setup form enhancements
            _manager.setupFormEnhancements();

            console.log('Interaction enhancements initialized');
        },
    
    // Enhanced touch interactions with better feedback
    setupEnhancedTouchInteractions: () => {
        const interactiveElements = document.querySelectorAll(
            'button, .nav-link, .dashboard-card, .dashboard-button, .submenu-link, a[href], .card, .btn'
        );

        interactiveElements.forEach(element => {
            // Add ripple class for better animations
            if (!element.classList.contains('ripple')) {
                element.classList.add('ripple');
            }
            
            // Enhanced touch start feedback
            element.addEventListener('touchstart', (e) => {
                element.style.setProperty('--touch-scale', '0.97');
                element.style.setProperty('--touch-opacity', '0.8');
                element.style.transform = 'scale(var(--touch-scale, 1))';
                element.style.opacity = 'var(--touch-opacity, 1)';
                
                // Add active class for CSS animations
                element.classList.add('touch-active');
            }, { passive: true });

            // Enhanced touch end feedback
            element.addEventListener('touchend', (e) => {
                setTimeout(() => {
                    element.style.removeProperty('--touch-scale');
                    element.style.removeProperty('--touch-opacity');
                    element.style.transform = '';
                    element.style.opacity = '';
                    element.classList.remove('touch-active');
                }, 150);
            }, { passive: true });

            // Handle touch cancel
            element.addEventListener('touchcancel', (e) => {
                element.style.removeProperty('--touch-scale');
                element.style.removeProperty('--touch-opacity');
                element.style.transform = '';
                element.style.opacity = '';
                element.classList.remove('touch-active');
            }, { passive: true });
        });
    },
    
    // Setup Material Design ripple effects
    setupRippleEffects: () => {
        document.addEventListener('click', (e) => {
            const element = e.target.closest('.ripple');
            if (!element) return;
            
            // Remove existing ripples
            const existingRipples = element.querySelectorAll('.ripple-effect');
            existingRipples.forEach(safeRemoveNode);
            
            // Create ripple element
            const ripple = document.createElement('span');
            ripple.classList.add('ripple-effect');
            
            // Calculate ripple size and position
            const rect = element.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;
            
            // Set ripple styles (use a sensible fallback color and make animation-friendly)
            ripple.style.cssText = `
                position: absolute;
                width: ${size}px;
                height: ${size}px;
                left: ${x}px;
                top: ${y}px;
                background: var(--focus-ring, rgba(15,23,42,0.10));
                border-radius: 50%;
                transform: scale(0);
                animation: ripple-animation 0.6s linear;
                pointer-events: none;
                z-index: 1;
                will-change: transform, opacity;
            `;
            
            // Ensure element has relative positioning
            if (getComputedStyle(element).position === 'static') {
                element.style.position = 'relative';
            }
            
            // Add ripple to element
            element.appendChild(ripple);
            
            // Remove ripple after animation
            setTimeout(() => safeRemoveNode(ripple), 600);
        });
        
            // Add ripple animation CSS
        const rippleCSS = `
            :root { --nexacrm-ripple: rgba(15,23,42,0.10); --nexacrm-ripple-weak: rgba(15,23,42,0.06); }
            @keyframes ripple-animation {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }

            .ripple { overflow: hidden; position: relative; }

            /* Improve ripple visuals and reduce visual weight for login toggle */
            .ripple-effect {
                transition: opacity 0.28s linear, transform 0.28s linear;
                opacity: 0.95;
                mix-blend-mode: normal;
                background: var(--nexacrm-ripple);
            }

            /* Make the ripple for the small login theme toggle lighter and subtler */
            .login-theme-toggle .ripple-effect {
                background: var(--nexacrm-ripple-weak) !important;
                opacity: 0.9 !important;
            }

            /* If the element author wants the ripple to be visually behind icon, lower z-index */
            .login-theme-toggle .ripple-effect { z-index: 0; }
        `;
        
        if (!document.getElementById('nexacrm-ripple-styles')) {
            const rippleStyle = document.createElement('style');
            rippleStyle.id = 'nexacrm-ripple-styles';
            rippleStyle.textContent = rippleCSS;
            document.head.appendChild(rippleStyle);
        }
    },
    
    // Setup swipe gestures for cards and navigation
    setupSwipeGestures: () => {
        let startX = 0;
        let startY = 0;
        let isSwipingHorizontally = false;
        
        document.addEventListener('touchstart', (e) => {
            const touch = e.touches[0];
            startX = touch.clientX;
            startY = touch.clientY;
            isSwipingHorizontally = false;
        }, { passive: true });
        
        document.addEventListener('touchmove', (e) => {
            if (!e.touches[0]) return;
            
            const touch = e.touches[0];
            const deltaX = touch.clientX - startX;
            const deltaY = touch.clientY - startY;
            
            // Determine if this is a horizontal swipe
            if (Math.abs(deltaX) > Math.abs(deltaY) && Math.abs(deltaX) > 10) {
                isSwipingHorizontally = true;
                
                // Handle sidebar swipe gestures
                const sidebar = document.querySelector('.sidebar');
                if (sidebar) {
                    // Swipe right to open menu (when starting from left edge)
                    if (startX < 50 && deltaX > 100) {
                        window.navigationHelper?.toggleMenu?.(false);
                    }
                    // Swipe left to close menu (when menu is open)
                    else if (!sidebar.classList.contains('collapse') && deltaX < -100) {
                        window.navigationHelper?.toggleMenu?.(true);
                    }
                }
            }
        }, { passive: true });
        
        document.addEventListener('touchend', (e) => {
            // Handle swipe actions for cards
            if (isSwipingHorizontally) {
                const card = e.target.closest('.dashboard-card, .card');
                if (card) {
                    card.style.transform = '';
                    card.style.transition = 'transform 0.3s ease';
                }
            }
            
            startX = 0;
            startY = 0;
            isSwipingHorizontally = false;
        }, { passive: true });
    },
    
    // Setup pull-to-refresh functionality
    setupPullToRefresh: () => {
        let startY = 0;
        let currentY = 0;
        let isPulling = false;
        let refreshThreshold = 70;
        
    // Only enable pull-to-refresh on touch devices and when debug flag is present.
    // Set window.__NEXACRM_DEBUG__ = true from server-side or dev HTML to enable during development.
    const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
    if (!isTouchDevice || !window.__NEXACRM_DEBUG__) return;

    const mainContent = document.querySelector('main') || document.querySelector('.content');
    if (!mainContent) return;
        
        // Create pull-to-refresh indicator with accessibility and animated spinner
        const refreshIndicator = document.createElement('div');
        refreshIndicator.className = 'pull-refresh-indicator';
        // ARIA: announce state changes to assistive tech
        refreshIndicator.setAttribute('role', 'status');
        refreshIndicator.setAttribute('aria-live', 'polite');
        refreshIndicator.setAttribute('aria-atomic', 'true');
        refreshIndicator.innerHTML = `
            <div class="refresh-icon" aria-hidden="true">
                <svg class="refresh-spinner" width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2" stroke-opacity="0.18"></circle>
                    <path class="refresh-spinner-head" d="M22 12a10 10 0 00-3.1-7.05" stroke="currentColor" stroke-width="2" stroke-linecap="round"></path>
                </svg>
            </div>
            <div class="refresh-text">Pull to refresh</div>
        `;
    // Make the indicator fixed at the top and more visible during debug.
            refreshIndicator.style.cssText = `
            position: fixed;
            top: 28px;
            left: 50%;
            transform: translateX(-50%) translateY(-6px);
            display: flex;
            flex-direction: row;
            align-items: center;
            gap: 10px;
            padding: 8px 14px;
            background: color-mix(in srgb, white 92%, var(--nexacrm-bg, #ffffff));
            color: var(--text-primary, #0f172a);
            border: 1px solid rgba(0,0,0,0.06);
            border-radius: 999px;
            box-shadow: 0 10px 30px rgba(2,6,23,0.12);
            transition: transform 0.22s cubic-bezier(.2,.9,.3,1), opacity 0.18s ease;
            z-index: 99999;
            pointer-events: none; /* non-interactive visual hint */
            opacity: 0; /* hidden by default until pulling */
        `;
        
        // Do not append into main flow; attach to body so it's always visible and fixed.
        try {
            document.body.appendChild(refreshIndicator);
        } catch (e) {
            mainContent.style.position = 'relative';
            mainContent.appendChild(refreshIndicator);
        }

        // Add spinner/check CSS if not present
        if (!document.getElementById('nexacrm-pull-refresh-styles')) {
            const rfStyle = document.createElement('style');
            rfStyle.id = 'nexacrm-pull-refresh-styles';
            rfStyle.textContent = `
                .pull-refresh-indicator { transition: transform 0.22s cubic-bezier(.2,.9,.3,1), opacity 0.18s ease; }
                .pull-refresh-indicator .refresh-spinner { display: block; color: var(--text-secondary, #6b7280); }
                .pull-refresh-indicator .refresh-spinner-head { transform-origin: center; }
                .pull-refresh-indicator.refreshing .refresh-spinner-head { animation: nexacrm-spin 1s linear infinite; }
                @keyframes nexacrm-spin { to { transform: rotate(360deg); } }
                .pull-refresh-indicator .refresh-check { display: none; }
                .pull-refresh-indicator.show-check .refresh-spinner { display: none; }
                .pull-refresh-indicator.show-check .refresh-check { display: block; }
                .pull-refresh-indicator .refresh-check svg { width:18px; height:18px; color:var(--success,#10b981); }
                /* Visible when pulling */
                .pull-refresh-indicator.visible { opacity: 1; }
            `;
            document.head.appendChild(rfStyle);
        }

        // Completion handler: listen for app-level completion event to animate success
        let completionTimeout = null;
        function handleRefreshComplete() {
            // show checkmark briefly
            refreshIndicator.classList.remove('refreshing');
            refreshIndicator.classList.add('show-check');
            const textEl = refreshIndicator.querySelector('.refresh-text');
            if (textEl) textEl.textContent = 'Refreshed';

            // clear any previous timeout
            if (completionTimeout) clearTimeout(completionTimeout);
            completionTimeout = setTimeout(() => {
                refreshIndicator.classList.remove('show-check');
                if (textEl) textEl.textContent = 'Pull to refresh';
            }, 900);
        }

        window.addEventListener('nexacrm:pullToRefreshComplete', handleRefreshComplete);
        
        mainContent.addEventListener('touchstart', (e) => {
            // only start when at top of scrollable container
            const atTop = mainContent.scrollTop === 0 || (window.scrollY === 0 && mainContent === document.body);
            if (atTop) {
                startY = e.touches[0].clientY;
                isPulling = true;
            }
        }, { passive: true });
        
        mainContent.addEventListener('touchmove', (e) => {
            if (!isPulling || mainContent.scrollTop > 0) return;
            
            currentY = e.touches[0].clientY;
            const pullDistance = Math.max(0, currentY - startY);
            
            if (pullDistance > 0) {
                e.preventDefault();
                
                // Update indicator transform and appearance
                const progress = Math.min(pullDistance / refreshThreshold, 1);
                refreshIndicator.style.transform = `translateX(-50%) translateY(${(Math.min(pullDistance, 100) * 0.4)}px)`;
                refreshIndicator.style.opacity = String(0.5 + (progress * 0.5));
                
                const icon = refreshIndicator.querySelector('.refresh-icon');
                const text = refreshIndicator.querySelector('.refresh-text');
                
                if (pullDistance >= refreshThreshold) {
                    icon.style.transform = 'rotate(180deg)';
                    text.textContent = 'Release to refresh';
                    refreshIndicator.style.color = 'var(--primary-color)';
                    refreshIndicator.classList.add('visible');
                } else {
                    icon.style.transform = `rotate(${progress * 180}deg)`;
                    text.textContent = 'Pull to refresh';
                    refreshIndicator.style.color = 'var(--text-secondary)';
                    refreshIndicator.classList.add('visible');
                }
            }
        }, { passive: false });
        
        mainContent.addEventListener('touchend', (e) => {
            if (!isPulling) return;
            
            const pullDistance = currentY - startY;
            
                if (pullDistance >= refreshThreshold) {
                    // Trigger refresh: dispatch an app-level event so Blazor or other code can handle it.
                    refreshIndicator.querySelector('.refresh-text').textContent = 'Refreshing...';
                    refreshIndicator.classList.add('refreshing');
                    refreshIndicator.classList.add('visible');

                    // Dispatch custom event for in-app handlers to perform a soft refresh.
                    try {
                        window.dispatchEvent(new CustomEvent('nexacrm:pullToRefresh'));
                    } catch (ex) {
                        // fallback to full reload only if dispatch fails
                        setTimeout(() => { window.location.reload(); }, 1200);
                    }
                }

            // Reset visual state
            refreshIndicator.style.transform = 'translateX(-50%) translateY(-6px)';
            refreshIndicator.classList.remove('visible');
            isPulling = false;
            startY = 0;
            currentY = 0;
        }, { passive: true });
    },
    
    // Setup scroll-based animations using Intersection Observer
    setupScrollAnimations: () => {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };
        
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                    entry.target.classList.remove('animate-out');
                } else {
                    entry.target.classList.add('animate-out');
                    entry.target.classList.remove('animate-in');
                }
            });
        }, observerOptions);
        
        // Observe cards and other animated elements
        const animatedElements = document.querySelectorAll('.dashboard-card, .card, .nav-item');
        animatedElements.forEach(element => {
            element.classList.add('animate-element');
            observer.observe(element);
        });
        
        // Add animation CSS
        const animationCSS = `
            .animate-element {
                opacity: 0;
                transform: translateY(20px);
                transition: all var(--transition-normal);
            }
            
            .animate-element.animate-in {
                opacity: 1;
                transform: translateY(0);
            }
            
            .animate-element.animate-out {
                opacity: 0.7;
                transform: translateY(10px);
            }
        `;
        
        if (!document.getElementById('nexacrm-animation-styles')) {
            const animationStyle = document.createElement('style');
            animationStyle.id = 'nexacrm-animation-styles';
            animationStyle.textContent = animationCSS;
            document.head.appendChild(animationStyle);
        }
    },
    
    // Enhanced keyboard navigation
    setupEnhancedKeyboardNavigation: () => {
        let focusedElementIndex = -1;
        const focusableElements = 'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])';
        
        document.addEventListener('keydown', (e) => {
            // Enhanced keyboard shortcuts
            switch (e.key) {
                case '/':
                    // Focus search if available
                    e.preventDefault();
                    const searchInput = document.querySelector('input[type="search"], input[placeholder*="search" i]');
                    if (searchInput) {
                        searchInput.focus();
                    }
                    break;
                    
                case 'Escape':
                    // Close modals, menus, etc.
                    const sidebar = document.querySelector('.sidebar');
                    if (sidebar && !sidebar.classList.contains('collapse')) {
                        window.navigationHelper?.toggleMenu?.(true);
                    }
                    
                    // Blur current element
                    if (document.activeElement) {
                        document.activeElement.blur();
                    }
                    break;
            }
        });
        
        // Add visual focus indicators
        document.addEventListener('focusin', (e) => {
            e.target.classList.add('keyboard-focused');
        });
        
        document.addEventListener('focusout', (e) => {
            e.target.classList.remove('keyboard-focused');
        });
    },
    
    // Form enhancements with real-time validation feedback
    setupFormEnhancements: () => {
        const forms = document.querySelectorAll('form');
        
        forms.forEach(form => {
            const inputs = form.querySelectorAll('input, select, textarea');
            
            inputs.forEach(input => {
                // Add floating label effect
                window.interactionManager.setupFloatingLabel(input);
                
                // Add real-time validation
                window.interactionManager.setupRealTimeValidation(input);
            });
        });
    },
    
    // Setup floating label effect for inputs
    setupFloatingLabel: (input) => {
        const label = document.querySelector(`label[for="${input.id}"]`);
        if (!label) return;
        
        // Add floating label CSS class
        label.classList.add('floating-label');
        input.parentElement.classList.add('floating-input-container');
        
        const updateLabelState = () => {
            if (input.value || input === document.activeElement) {
                label.classList.add('floating');
            } else {
                label.classList.remove('floating');
            }
        };
        
        input.addEventListener('focus', updateLabelState);
        input.addEventListener('blur', updateLabelState);
        input.addEventListener('input', updateLabelState);
        
        // Initial state
        updateLabelState();
        
        // Add floating label CSS
        const floatingLabelCSS = `
            .floating-input-container {
                position: relative;
                margin-bottom: var(--spacing-lg);
            }
            
            .floating-label {
                position: absolute;
                left: var(--spacing-sm);
                top: 50%;
                transform: translateY(-50%);
                color: var(--text-secondary);
                pointer-events: none;
                transition: all var(--transition-fast);
                background: var(--background-color);
                padding: 0 var(--spacing-xs);
                z-index: 1;
            }
            
            .floating-label.floating {
                top: 0;
                left: var(--spacing-sm);
                transform: translateY(-50%);
                font-size: 0.8rem;
                color: var(--primary-color);
            }
            
            .floating-input-container input,
            .floating-input-container select,
            .floating-input-container textarea {
                background: transparent;
                border: 1px solid var(--border-color);
                padding: var(--spacing-sm);
                width: 100%;
            }
            
            .floating-input-container input:focus,
            .floating-input-container select:focus,
            .floating-input-container textarea:focus {
                border-color: var(--primary-color);
                outline: none;
            }
        `;
        
        if (!document.querySelector('#floating-label-styles')) {
            const floatingLabelStyle = document.createElement('style');
            floatingLabelStyle.id = 'floating-label-styles';
            floatingLabelStyle.textContent = floatingLabelCSS;
            document.head.appendChild(floatingLabelStyle);
        }
    },
    
    // Setup real-time validation feedback
    setupRealTimeValidation: (input) => {
        let validationTimeout;
        
        input.addEventListener('input', () => {
            clearTimeout(validationTimeout);
            
            // Remove existing validation classes
            input.classList.remove('validation-success', 'validation-error');
            
            // Debounce validation
            validationTimeout = setTimeout(() => {
                const isValid = input.checkValidity();
                
                if (input.value.trim()) {
                    input.classList.add(isValid ? 'validation-success' : 'validation-error');
                    
                    // Show validation message
                    window.interactionManager.showValidationFeedback(input, isValid);
                }
            }, 300);
        });
        
        // Add validation CSS
        const validationCSS = `
            .validation-success {
                border-color: #10b981 !important;
                box-shadow: 0 0 0 2px rgba(16, 185, 129, 0.2) !important;
            }
            
            .validation-error {
                border-color: #ef4444 !important;
                box-shadow: 0 0 0 2px rgba(239, 68, 68, 0.2) !important;
            }
            
            .validation-feedback {
                font-size: 0.8rem;
                margin-top: var(--spacing-xs);
                transition: all var(--transition-fast);
            }
            
            .validation-feedback.success {
                color: #10b981;
            }
            
            .validation-feedback.error {
                color: #ef4444;
            }
        `;
        
        if (!document.querySelector('#validation-styles')) {
            const validationStyle = document.createElement('style');
            validationStyle.id = 'validation-styles';
            validationStyle.textContent = validationCSS;
            document.head.appendChild(validationStyle);
        }
    },
    
    // Show validation feedback
    showValidationFeedback: (input, isValid) => {
        let feedback = input.parentElement.querySelector('.validation-feedback');
        
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'validation-feedback';
            input.parentElement.appendChild(feedback);
        }
        
        feedback.className = `validation-feedback ${isValid ? 'success' : 'error'}`;
        feedback.textContent = isValid ? '✓ Looks good' : input.validationMessage;
    }
    };

    // mark and expose
    _manager._nexacrm_init = true;
    window.interactionManager = Object.assign(window.interactionManager || {}, _manager);
})();

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.interactionManager.init);
} else {
    window.interactionManager.init();
}

// Provide a guarded global downloadFile helper to avoid duplicate redefinition
if (!(window.downloadFile && window.downloadFile._nexacrm_init)) {
    window.downloadFile = (fileName, content) => {
        const blob = new Blob([content], { type: 'text/csv' });

        if (navigator.share && navigator.canShare && navigator.canShare({ files: [new File([blob], fileName, { type: 'text/csv' })] })) {
            const file = new File([blob], fileName, { type: 'text/csv' });
            navigator.share({ files: [file] }).catch(() => fallbackDownload(blob));
            return;
        }

        fallbackDownload(blob);

        function fallbackDownload(b) {
            const url = URL.createObjectURL(b);
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            // Defensive removal: the link may have been removed elsewhere or
            // not appended in some environments. Check parentNode first.
            safeRemoveNode(link);
            // Revoke the object URL if available. Guard in a try/catch to
            // avoid unhandled exceptions in older or restricted environments.
            try {
                if (typeof URL !== 'undefined' && url) {
                    URL.revokeObjectURL(url);
                }
            } catch (e) {
                console.warn('Failed to revoke object URL', e);
            }
        }
    };
    // mark initialized to avoid later overwrites when scripts are re-evaluated
    try { window.downloadFile._nexacrm_init = true; } catch (e) { /* ignore */ }

    try { window.downloadFile._nexacrm_init = true; } catch (e) { /* ignore */ }
}
