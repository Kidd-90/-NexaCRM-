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
            button.addEventListener('click', (e) => {
                e.preventDefault();
                window.themeManager.toggleTheme();
            });
        });
        
        // Setup keyboard shortcut (Ctrl/Cmd + Shift + T)
        document.addEventListener('keydown', (e) => {
            if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'T') {
                e.preventDefault();
                window.themeManager.toggleTheme();
            }
        });
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
            // Update button icon or text based on current theme
            const lightIcon = button.querySelector('.theme-light-icon');
            const darkIcon = button.querySelector('.theme-dark-icon');
            
            if (lightIcon && darkIcon) {
                if (currentTheme === window.themeManager.THEMES.DARK) {
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
            button.setAttribute('data-current-theme', currentTheme);
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