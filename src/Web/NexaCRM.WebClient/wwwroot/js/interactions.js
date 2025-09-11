// Enhanced Interactions and Micro-animations for NexaCRM
window.interactionManager = {
    // Initialize all interaction enhancements
    init: () => {
        console.log('Initializing interaction enhancements...');
        
        // Setup enhanced touch interactions
        window.interactionManager.setupEnhancedTouchInteractions();
        
        // Setup ripple effects
        window.interactionManager.setupRippleEffects();
        
        // Setup swipe gestures
        window.interactionManager.setupSwipeGestures();
        
        // Setup pull-to-refresh
        window.interactionManager.setupPullToRefresh();
        
        // Setup intersection observer for animations
        window.interactionManager.setupScrollAnimations();
        
        // Setup enhanced keyboard navigation
        window.interactionManager.setupEnhancedKeyboardNavigation();
        
        // Setup form enhancements
        window.interactionManager.setupFormEnhancements();
        
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
            existingRipples.forEach(ripple => ripple.remove());
            
            // Create ripple element
            const ripple = document.createElement('span');
            ripple.classList.add('ripple-effect');
            
            // Calculate ripple size and position
            const rect = element.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;
            
            // Set ripple styles
            ripple.style.cssText = `
                position: absolute;
                width: ${size}px;
                height: ${size}px;
                left: ${x}px;
                top: ${y}px;
                background: var(--focus-ring);
                border-radius: 50%;
                transform: scale(0);
                animation: ripple-animation 0.6s linear;
                pointer-events: none;
                z-index: 1;
            `;
            
            // Ensure element has relative positioning
            if (getComputedStyle(element).position === 'static') {
                element.style.position = 'relative';
            }
            
            // Add ripple to element
            element.appendChild(ripple);
            
            // Remove ripple after animation
            setTimeout(() => ripple.remove(), 600);
        });
        
        // Add ripple animation CSS
        const rippleCSS = `
            @keyframes ripple-animation {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }
            
            .ripple {
                overflow: hidden;
            }
        `;
        
        const styleSheet = document.createElement('style');
        styleSheet.textContent = rippleCSS;
        document.head.appendChild(styleSheet);
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
        
        const mainContent = document.querySelector('main') || document.querySelector('.content');
        if (!mainContent) return;
        
        // Create pull-to-refresh indicator
        const refreshIndicator = document.createElement('div');
        refreshIndicator.className = 'pull-refresh-indicator';
        refreshIndicator.innerHTML = '<div class="refresh-icon">↓</div><div class="refresh-text">Pull to refresh</div>';
        refreshIndicator.style.cssText = `
            position: absolute;
            top: -60px;
            left: 50%;
            transform: translateX(-50%);
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 8px;
            padding: 10px;
            background: var(--surface-color);
            border-radius: 8px;
            box-shadow: 0 2px 8px var(--shadow-light);
            transition: all 0.3s ease;
            z-index: 1000;
        `;
        
        mainContent.style.position = 'relative';
        mainContent.appendChild(refreshIndicator);
        
        mainContent.addEventListener('touchstart', (e) => {
            if (mainContent.scrollTop === 0) {
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
                
                // Update indicator position and appearance
                const progress = Math.min(pullDistance / refreshThreshold, 1);
                refreshIndicator.style.top = `${-60 + (pullDistance * 0.5)}px`;
                refreshIndicator.style.opacity = progress;
                
                const icon = refreshIndicator.querySelector('.refresh-icon');
                const text = refreshIndicator.querySelector('.refresh-text');
                
                if (pullDistance >= refreshThreshold) {
                    icon.style.transform = 'rotate(180deg)';
                    text.textContent = 'Release to refresh';
                    refreshIndicator.style.color = 'var(--primary-color)';
                } else {
                    icon.style.transform = `rotate(${progress * 180}deg)`;
                    text.textContent = 'Pull to refresh';
                    refreshIndicator.style.color = 'var(--text-secondary)';
                }
            }
        }, { passive: false });
        
        mainContent.addEventListener('touchend', (e) => {
            if (!isPulling) return;
            
            const pullDistance = currentY - startY;
            
            if (pullDistance >= refreshThreshold) {
                // Trigger refresh
                refreshIndicator.querySelector('.refresh-text').textContent = 'Refreshing...';
                refreshIndicator.querySelector('.refresh-icon').style.animation = 'spin 1s linear infinite';
                
                // Simulate refresh (replace with actual refresh logic)
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            }
            
            // Reset
            refreshIndicator.style.top = '-60px';
            refreshIndicator.style.opacity = '0';
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
        
        const styleSheet = document.createElement('style');
        styleSheet.textContent = animationCSS;
        document.head.appendChild(styleSheet);
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
            const styleSheet = document.createElement('style');
            styleSheet.id = 'floating-label-styles';
            styleSheet.textContent = floatingLabelCSS;
            document.head.appendChild(styleSheet);
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
            const styleSheet = document.createElement('style');
            styleSheet.id = 'validation-styles';
            styleSheet.textContent = validationCSS;
            document.head.appendChild(styleSheet);
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

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.interactionManager.init);
} else {
    window.interactionManager.init();
}