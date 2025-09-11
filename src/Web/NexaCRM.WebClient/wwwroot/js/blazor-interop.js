// Blazor 상호 운용성을 위한 JavaScript 함수들

window.blazorInterop = {
    // DOM 요소 조작
    setElementAttribute: function(elementId, attribute, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.setAttribute(attribute, value);
        }
    },
    
    removeElementAttribute: function(elementId, attribute) {
        const element = document.getElementById(elementId);
        if (element) {
            element.removeAttribute(attribute);
        }
    },
    
    addElementClass: function(elementId, className) {
        const element = document.getElementById(elementId);
        if (element) {
            element.classList.add(className);
        }
    },
    
    removeElementClass: function(elementId, className) {
        const element = document.getElementById(elementId);
        if (element) {
            element.classList.remove(className);
        }
    },
    
    toggleElementClass: function(elementId, className) {
        const element = document.getElementById(elementId);
        if (element) {
            element.classList.toggle(className);
            return element.classList.contains(className);
        }
        return false;
    },
    
    // 포커스 관리
    focusElement: function(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
        }
    },
    
    // 클립보드 관리
    copyToClipboard: function(text) {
        navigator.clipboard.writeText(text).then(() => {
            return true;
        }).catch(() => {
            return false;
        });
    },
    
    // 브라우저 정보
    getBrowserInfo: function() {
        return {
            userAgent: navigator.userAgent,
            language: navigator.language,
            platform: navigator.platform,
            cookieEnabled: navigator.cookieEnabled,
            onLine: navigator.onLine
        };
    },
    
    // 화면 크기 정보
    getScreenInfo: function() {
        return {
            width: window.innerWidth,
            height: window.innerHeight,
            availWidth: screen.availWidth,
            availHeight: screen.availHeight
        };
    },
    
    // 현재 URL 정보
    getCurrentUrl: function() {
        return window.location.href;
    },
    
    // URL 변경
    navigateTo: function(url) {
        window.location.href = url;
    },
    
    // 새 탭에서 열기
    openInNewTab: function(url) {
        window.open(url, '_blank');
    },
    
    // 페이지 새로고침
    refreshPage: function() {
        window.location.reload();
    }
};
