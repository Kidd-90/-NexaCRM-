// Theme Manager for NexaCRM - Handles dark/light mode switching
window.themeManager = {
    // Theme constants
    THEMES: {
        LIGHT: 'light',
        DARK: 'dark',
        AUTO: 'auto'
    },
    
    STORAGE_KEY: 'nexacrm-theme-preference',
    
    // Current theme state
    currentTheme: 'light',
    
    // MutationObserver for dynamic theme toggle buttons
    themeObserver: null,
    
    // Initialize theme system
    init: () => {
        // Get saved preference or default to auto
        const savedTheme = localStorage.getItem(window.themeManager.STORAGE_KEY) || window.themeManager.THEMES.AUTO;
        
        // Set up system theme detection
        window.themeManager.setupSystemThemeDetection();
        
        // Apply initial theme
        window.themeManager.setTheme(savedTheme);
        
        // Setup theme toggle listeners
        window.themeManager.setupThemeToggleListeners();
        
        // Update theme toggle buttons immediately after initialization
        setTimeout(() => {
            window.themeManager.updateThemeToggleButtons();
        }, 100);
        
        console.log('Theme Manager initialized with theme:', savedTheme);
    },
    
    // Set theme with smooth transition
    setTheme: (theme) => {
        let actualTheme = theme;
        
        // Handle auto theme
        if (theme === window.themeManager.THEMES.AUTO) {
            actualTheme = window.themeManager.getSystemTheme();
        }
        
        // Add transition class to prevent flash
        document.documentElement.classList.add('theme-transitioning');
        
        // Apply theme
        if (actualTheme === window.themeManager.THEMES.DARK) {
            document.documentElement.setAttribute('data-theme', 'dark');
        } else {
            document.documentElement.removeAttribute('data-theme');
        }
        
        // Update current theme
        window.themeManager.currentTheme = actualTheme;
        
        // Save preference (save original preference, not resolved theme)
        localStorage.setItem(window.themeManager.STORAGE_KEY, theme);
        
        // Remove transition class after animation
        setTimeout(() => {
            document.documentElement.classList.remove('theme-transitioning');
        }, 300);
        
        // Update theme toggle buttons
        setTimeout(() => {
            window.themeManager.updateThemeToggleButtons();
        }, 50);
        
        // Dispatch theme change event
        window.dispatchEvent(new CustomEvent('themeChanged', {
            detail: { theme: actualTheme, preference: theme }
        }));
        
        console.log('Theme changed to:', actualTheme);
    },
    
    // Get current system theme
    getSystemTheme: () => {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return window.themeManager.THEMES.DARK;
        }
        return window.themeManager.THEMES.LIGHT;
    },
    
    // Setup system theme change detection
    setupSystemThemeDetection: () => {
        if (window.matchMedia) {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
            
            // Listen for system theme changes
            mediaQuery.addEventListener('change', (e) => {
                const currentPreference = localStorage.getItem(window.themeManager.STORAGE_KEY);
                
                // Only auto-update if user preference is set to auto
                if (currentPreference === window.themeManager.THEMES.AUTO) {
                    const newTheme = e.matches ? window.themeManager.THEMES.DARK : window.themeManager.THEMES.LIGHT;
                    window.themeManager.setTheme(window.themeManager.THEMES.AUTO);
                }
            });
        }
    },
    
    // Setup theme toggle button listeners
    setupThemeToggleListeners: () => {
        // Look for theme toggle buttons
        const themeToggleButtons = document.querySelectorAll('[data-theme-toggle]');
        
        themeToggleButtons.forEach(button => {
            // Check if listener is already attached to avoid duplicates
            if (!button.hasAttribute('data-theme-listener-attached')) {
                button.addEventListener('click', (e) => {
                    e.preventDefault();
                    console.log('Theme toggle button clicked!');
                    window.themeManager.toggleTheme();
                });
                button.setAttribute('data-theme-listener-attached', 'true');
                console.log('Theme toggle listener attached to button:', button);
            }
        });
        
        console.log(`Found ${themeToggleButtons.length} theme toggle buttons`);
        
            // Setup keyboard shortcut (Ctrl/Cmd + Shift + T)
            if (!document.documentElement.hasAttribute('data-theme-keyboard-listener')) {
                document.addEventListener('keydown', (e) => {
                    if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'T') {
                        e.preventDefault();
                        window.themeManager.toggleTheme();
                    }
                });
                document.documentElement.setAttribute('data-theme-keyboard-listener', 'true');
            }
        
        // Set up MutationObserver to watch for dynamically added theme toggle buttons
        if (!window.themeManager.themeObserver) {
            window.themeManager.themeObserver = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    mutation.addedNodes.forEach((node) => {
                        if (node.nodeType === Node.ELEMENT_NODE) {
                            // Check if the added node itself has data-theme-toggle
                            if (node.hasAttribute && node.hasAttribute('data-theme-toggle')) {
                                if (!node.hasAttribute('data-theme-listener-attached')) {
                                    node.addEventListener('click', (e) => {
                                        e.preventDefault();
                                        console.log('Theme toggle button clicked (from MutationObserver)!');
                                        window.themeManager.toggleTheme();
                                    });
                                    node.setAttribute('data-theme-listener-attached', 'true');
                                    console.log('MutationObserver: Theme toggle listener attached to new button:', node);
                                }
                            }
                            
                            // Check for theme toggle buttons within the added node
                            if (node.querySelectorAll) {
                                const newToggleButtons = node.querySelectorAll('[data-theme-toggle]');
                                newToggleButtons.forEach(button => {
                                    if (!button.hasAttribute('data-theme-listener-attached')) {
                                        button.addEventListener('click', (e) => {
                                            e.preventDefault();
                                            console.log('Theme toggle button clicked (from MutationObserver child)!');
                                            window.themeManager.toggleTheme();
                                        });
                                        button.setAttribute('data-theme-listener-attached', 'true');
                                        console.log('MutationObserver: Theme toggle listener attached to child button:', button);
                                    }
                                });
                                
                                if (newToggleButtons.length > 0) {
                                    console.log(`MutationObserver: Found ${newToggleButtons.length} new theme toggle buttons`);
                                }
                            }
                        }
                    });
                });
            });
            
            // Start observing
            window.themeManager.themeObserver.observe(document.body, {
                childList: true,
                subtree: true
            });
        }
    },
    
    // Toggle between light and dark theme
    toggleTheme: () => {
        const currentPreference = localStorage.getItem(window.themeManager.STORAGE_KEY) || window.themeManager.THEMES.AUTO;
        
        let newTheme;
        switch (currentPreference) {
            case window.themeManager.THEMES.LIGHT:
                newTheme = window.themeManager.THEMES.DARK;
                break;
            case window.themeManager.THEMES.DARK:
                newTheme = window.themeManager.THEMES.LIGHT;
                break;
            case window.themeManager.THEMES.AUTO:
                // If auto, switch to opposite of current system theme
                const systemTheme = window.themeManager.getSystemTheme();
                newTheme = systemTheme === window.themeManager.THEMES.DARK ? 
                    window.themeManager.THEMES.LIGHT : window.themeManager.THEMES.DARK;
                break;
            default:
                newTheme = window.themeManager.THEMES.LIGHT;
        }
        
        window.themeManager.setTheme(newTheme);
    },
    
    // Get current theme preference
    getThemePreference: () => {
        return localStorage.getItem(window.themeManager.STORAGE_KEY) || window.themeManager.THEMES.AUTO;
    },
    
    // Get current active theme
    getCurrentTheme: () => {
        return window.themeManager.currentTheme;
    },
    
    // Update theme toggle button appearance
    updateThemeToggleButtons: () => {
        const themeToggleButtons = document.querySelectorAll('[data-theme-toggle]');
        const currentTheme = window.themeManager.currentTheme;
        
        themeToggleButtons.forEach(button => {
            window.themeManager.updateSingleThemeToggleButton(button, currentTheme);
        });
    },
    
    // Update a single theme toggle button
    updateSingleThemeToggleButton: (button, currentTheme = null) => {
        if (!button) return;
        
        const theme = currentTheme || window.themeManager.currentTheme;
        const lightIcon = button.querySelector('.theme-light-icon');
        const darkIcon = button.querySelector('.theme-dark-icon');
        
        if (lightIcon && darkIcon) {
            if (theme === window.themeManager.THEMES.DARK) {
                lightIcon.style.display = 'inline';
                darkIcon.style.display = 'none';
                button.setAttribute('aria-label', 'Switch to light theme');
            } else {
                lightIcon.style.display = 'none';
                darkIcon.style.display = 'inline';
                button.setAttribute('aria-label', 'Switch to dark theme');
            }
        }
        
        // Update button state
        button.setAttribute('data-current-theme', theme);
    },
    
    // Clean up observers and listeners (useful for testing or cleanup)
    cleanup: () => {
        if (window.themeManager.themeObserver) {
            window.themeManager.themeObserver.disconnect();
            window.themeManager.themeObserver = null;
        }
        
        // Remove listener flags so they can be re-attached if needed
        document.removeAttribute('data-theme-keyboard-listener');
        
        const themeButtons = document.querySelectorAll('[data-theme-toggle]');
        themeButtons.forEach(button => {
            button.removeAttribute('data-theme-listener-attached');
        });
    }
};

// Add smooth theme transition CSS
const themeTransitionCSS = `
    .theme-transitioning * {
        transition: background-color 0.3s cubic-bezier(0.4, 0, 0.2, 1), 
                    color 0.3s cubic-bezier(0.4, 0, 0.2, 1),
                    border-color 0.3s cubic-bezier(0.4, 0, 0.2, 1),
                    box-shadow 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
    }
`;

// Inject transition CSS
const styleSheet = document.createElement('style');
styleSheet.textContent = themeTransitionCSS;
document.head.appendChild(styleSheet);

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.themeManager.init);
} else {
    window.themeManager.init();
}

// Listen for theme changes to update toggle buttons
window.addEventListener('themeChanged', () => {
    window.themeManager.updateThemeToggleButtons();
});

// Listen for Blazor navigation events to re-setup theme toggle listeners
window.addEventListener('blazorNavigated', () => {
    // Re-setup theme toggle listeners after navigation
    setTimeout(() => {
        window.themeManager.setupThemeToggleListeners();
    }, 100);
});

// Also listen for general page changes/updates
document.addEventListener('DOMContentLoaded', () => {
    // Setup a periodic check to ensure theme toggle buttons are working
    // This is a fallback in case MutationObserver misses some dynamic elements
    setInterval(() => {
        const themeButtons = document.querySelectorAll('[data-theme-toggle]:not([data-theme-listener-attached])');
        if (themeButtons.length > 0) {
            console.log('Found unattached theme toggle buttons, attaching listeners...');
            window.themeManager.setupThemeToggleListeners();
        }
    }, 2000); // Check every 2 seconds
});