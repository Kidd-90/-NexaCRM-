// Navigation menu functionality
window.navigationHelper = {
    // 오버레이 클릭 시 메뉴 닫기
    setupOverlayHandler: () => {
        const overlay = document.querySelector('.mobile-overlay');
        const sidebar = document.querySelector('.sidebar');
        
        if (overlay && sidebar) {
            overlay.addEventListener('click', () => {
                // 사이드바에 collapse 클래스 추가하여 메뉴 닫기
                sidebar.classList.add('collapse');
            });
        }
        
        // 초기 상태에서 메뉴를 확실히 숨김
        if (sidebar && !sidebar.classList.contains('collapse')) {
            sidebar.classList.add('collapse');
        }
    },
    
    // 컴포넌트에서 호출할 수 있는 메뉴 토글 함수
    toggleMenu: (isCollapsed) => {
        const sidebar = document.querySelector('.sidebar');
        if (sidebar) {
            if (isCollapsed) {
                sidebar.classList.add('collapse');
            } else {
                sidebar.classList.remove('collapse');
            }
        }
    },
    
    // 페이지 로드 시 초기화
    initialize: () => {
        // DOM이 준비되면 오버레이 핸들러 설정
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', window.navigationHelper.setupOverlayHandler);
        } else {
            window.navigationHelper.setupOverlayHandler();
        }
        
        // 즉시 실행하여 초기 상태 보장
        setTimeout(() => {
            const sidebar = document.querySelector('.sidebar');
            if (sidebar && !sidebar.classList.contains('collapse')) {
                sidebar.classList.add('collapse');
            }
        }, 100);
    }
};

// 초기화 실행
window.navigationHelper.initialize();
