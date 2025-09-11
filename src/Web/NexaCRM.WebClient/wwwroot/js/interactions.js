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
                element.classList.add('touch-feedback');
                element.style.transform = 'scale(0.98)';
                element.style.opacity = '0.8';
            }, { passive: true });

            // Enhanced touch end feedback
            element.addEventListener('touchend', (e) => {
                setTimeout(() => {
                    element.classList.remove('touch-feedback');
                    element.style.transform = '';
                    element.style.opacity = '';
                }, 150);
            }, { passive: true });

            // Handle touch cancel
            element.addEventListener('touchcancel', (e) => {
                element.classList.remove('touch-feedback');
                element.style.transform = '';
                element.style.opacity = '';
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
                left: ${x}px;
                top: ${y}px;
                width: ${size}px;
                height: ${size}px;
                border-radius: 50%;
                background: rgba(255, 255, 255, 0.6);
                transform: scale(0);
                animation: ripple-animation 0.6s linear;
                pointer-events: none;
                z-index: 1;
            `;
            
            element.appendChild(ripple);
            
            // Remove ripple after animation
            setTimeout(() => {
                ripple.remove();
            }, 600);
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
                position: relative;
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
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
            isSwipingHorizontally = false;
        }, { passive: true });
        
        document.addEventListener('touchmove', (e) => {
            if (!startX || !startY) return;
            
            const currentX = e.touches[0].clientX;
            const currentY = e.touches[0].clientY;
            const deltaX = Math.abs(currentX - startX);
            const deltaY = Math.abs(currentY - startY);
            
            // Determine swipe direction
            if (deltaX > deltaY && deltaX > 10) {
                isSwipingHorizontally = true;
            }
        }, { passive: true });
        
        document.addEventListener('touchend', (e) => {
            if (!startX || !startY || !isSwipingHorizontally) return;
            
            const endX = e.changedTouches[0].clientX;
            const deltaX = endX - startX;
            
            // Handle swipe actions
            if (Math.abs(deltaX) > 50) {
                const swipeTarget = e.target.closest('.swipeable, .dashboard-card, .nav-item');
                if (swipeTarget) {
                    if (deltaX > 0) {
                        // Swipe right
                        swipeTarget.dispatchEvent(new CustomEvent('swipeRight'));
                    } else {
                        // Swipe left
                        swipeTarget.dispatchEvent(new CustomEvent('swipeLeft'));
                    }
                }
            }
            
            // Reset values
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
            if (!isPulling) return;
            
            currentY = e.touches[0].clientY;
            const pullDistance = Math.max(0, currentY - startY);
            
            if (pullDistance > 0) {
                e.preventDefault();
                const translateY = Math.min(pullDistance * 0.5, refreshThreshold);
                mainContent.style.transform = `translateY(${translateY}px)`;
                
                // Update indicator
                refreshIndicator.style.top = `${translateY - 60}px`;
                refreshIndicator.style.opacity = Math.min(translateY / refreshThreshold, 1);
                
                if (translateY >= refreshThreshold) {
                    refreshIndicator.innerHTML = '<div class="refresh-icon">↑</div><div class="refresh-text">Release to refresh</div>';
                } else {
                    refreshIndicator.innerHTML = '<div class="refresh-icon">↓</div><div class="refresh-text">Pull to refresh</div>';
                }
            }
        }, { passive: false });
        
        mainContent.addEventListener('touchend', (e) => {
            if (!isPulling) return;
            
            const pullDistance = Math.max(0, currentY - startY);
            
            if (pullDistance >= refreshThreshold) {
                // Trigger refresh
                refreshIndicator.innerHTML = '<div class="refresh-icon spinning">⟳</div><div class="refresh-text">Refreshing...</div>';
                
                // Dispatch refresh event
                window.dispatchEvent(new CustomEvent('pullToRefresh'));
                
                setTimeout(() => {
                    mainContent.style.transform = '';
                    refreshIndicator.style.top = '-60px';
                    refreshIndicator.style.opacity = '0';
                    refreshIndicator.innerHTML = '<div class="refresh-icon">↓</div><div class="refresh-text">Pull to refresh</div>';
                }, 1000);
            } else {
                // Reset
                mainContent.style.transform = '';
                refreshIndicator.style.top = '-60px';
                refreshIndicator.style.opacity = '0';
            }
            
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
            const focusableElementsList = document.querySelectorAll(focusableElements);
            const currentIndex = Array.from(focusableElementsList).indexOf(document.activeElement);
            
            switch (e.key) {
                case 'Tab':
                    // Enhanced tab navigation with visual feedback
                    if (document.activeElement) {
                        document.activeElement.classList.add('keyboard-focused');
                        setTimeout(() => {
                            if (document.activeElement) {
                                document.activeElement.classList.remove('keyboard-focused');
                            }
                        }, 2000);
                    }
                    break;
                    
                case 'Enter':
                    // Enhanced enter key handling
                    if (document.activeElement && document.activeElement.classList.contains('keyboard-focused')) {
                        document.activeElement.click();
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
                // Setup floating labels
                window.interactionManager.setupFloatingLabel(input);
                
                // Setup real-time validation
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
            validationTimeout = setTimeout(() => {
                const isValid = input.checkValidity();
                input.classList.toggle('validation-success', isValid);
                input.classList.toggle('validation-error', !isValid);
                
                window.interactionManager.showValidationFeedback(input, isValid);
            }, 500);
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
