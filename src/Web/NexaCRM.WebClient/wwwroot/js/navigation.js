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
            document.addEventListener('DOMContentLoaded', () => {
                window.navigationHelper.setupOverlayHandler();
                window.navigationHelper.setupAutoLogout();
            });
        } else {
            window.navigationHelper.setupOverlayHandler();
            window.navigationHelper.setupAutoLogout();
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
