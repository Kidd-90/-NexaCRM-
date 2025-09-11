// Blazor WebAssembly 앱용 공용 JavaScript 함수들

// 테마 관리
window.blazorCulture = {
    get: () => window.localStorage['BlazorCulture'],
    set: (value) => window.localStorage['BlazorCulture'] = value
};

// 다크 모드 테마 토글
window.themeManager = {
    toggleTheme: function() {
        const body = document.body;
        const currentTheme = body.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        
        body.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        
        return newTheme;
    },
    
    initializeTheme: function() {
        const savedTheme = localStorage.getItem('theme') || 'light';
        document.body.setAttribute('data-theme', savedTheme);
        return savedTheme;
    },
    
    getCurrentTheme: function() {
        return document.body.getAttribute('data-theme') || 'light';
    }
};

// 모바일 네비게이션 메뉴 토글
window.mobileMenu = {
    toggle: function() {
        const navMenu = document.querySelector('.nav-menu');
        const overlay = document.querySelector('.nav-overlay');
        
        if (navMenu && overlay) {
            navMenu.classList.toggle('show');
            overlay.classList.toggle('show');
            document.body.classList.toggle('menu-open');
        }
    },
    
    close: function() {
        const navMenu = document.querySelector('.nav-menu');
        const overlay = document.querySelector('.nav-overlay');
        
        if (navMenu && overlay) {
            navMenu.classList.remove('show');
            overlay.classList.remove('show');
            document.body.classList.remove('menu-open');
        }
    }
};

// Bootstrap 모달 지원
window.bootstrapModal = {
    show: function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            const bootstrapModal = new bootstrap.Modal(modal);
            bootstrapModal.show();
        }
    },
    
    hide: function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            const bootstrapModal = bootstrap.Modal.getInstance(modal);
            if (bootstrapModal) {
                bootstrapModal.hide();
            }
        }
    }
};

// 스크롤 관리
window.scrollManager = {
    scrollToTop: function() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    
    scrollToElement: function(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth' });
        }
    }
};

// 로컬 스토리지 관리
window.localStorage = {
    setItem: function(key, value) {
        window.localStorage.setItem(key, value);
    },
    
    getItem: function(key) {
        return window.localStorage.getItem(key);
    },
    
    removeItem: function(key) {
        window.localStorage.removeItem(key);
    },
    
    clear: function() {
        window.localStorage.clear();
    }
};

// Blazor 시작 시 테마 초기화
document.addEventListener('DOMContentLoaded', function() {
    window.themeManager.initializeTheme();
});

// 앱 초기화
window.blazorApp = {
    initialize: function() {
        // 테마 초기화
        window.themeManager.initializeTheme();
        
        // 모바일 메뉴 오버레이 클릭 시 메뉴 닫기
        document.addEventListener('click', function(e) {
            if (e.target.classList.contains('nav-overlay')) {
                window.mobileMenu.close();
            }
        });
        
        // ESC 키로 모바일 메뉴 닫기
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                window.mobileMenu.close();
            }
        });
    }
};
