// Authentication and session management
(function(){
    if (window.authManager && window.authManager._nexacrm_init) {
        console.debug('authManager already initialized; skipping');
        return;
    }

    const _auth = {
    // 세션 타임아웃: 30분 (밀리초)
    SESSION_TIMEOUT: 30 * 60 * 1000,

    // 로컬 스토리지 키
    STORAGE_KEYS: {
        USERNAME: 'username',
        ROLES: 'roles',
        DEVELOPER: 'isDeveloper'
    },

    // 타이머 변수들
    sessionTimeoutId: null,
    lastActivityTime: Date.now(),

    // 인증 상태 확인
    isAuthenticated: () => {
        const username = localStorage.getItem(window.authManager.STORAGE_KEYS.USERNAME);
        const roles = localStorage.getItem(window.authManager.STORAGE_KEYS.ROLES);
        const developerFlag = localStorage.getItem(window.authManager.STORAGE_KEYS.DEVELOPER);

        if (!username || username === 'null') {
            return false;
        }

        if (roles && roles !== 'null') {
            return true;
        }

        return developerFlag === 'true';
    },

    // Developer 역할 확인
    isDeveloper: () => {
        const developerFlag = localStorage.getItem(window.authManager.STORAGE_KEYS.DEVELOPER);
        if (developerFlag === 'true') {
            return true;
        }

        const roles = localStorage.getItem(window.authManager.STORAGE_KEYS.ROLES);
        if (!roles || roles === 'null') {
            return false;
        }

        try {
            const parsedRoles = JSON.parse(roles);
            if (Array.isArray(parsedRoles)) {
                return parsedRoles.some(role => typeof role === 'string' && role.toLowerCase() === 'developer');
            }
        } catch (error) {
            console.warn('Failed to parse stored roles for developer detection', error);
        }

        return false;
    },

    // 로그아웃 처리
    logout: (reason = 'manual') => {
        console.log(`Logging out user: ${reason}`);

        // localStorage 정리
        localStorage.removeItem(window.authManager.STORAGE_KEYS.USERNAME);
        localStorage.removeItem(window.authManager.STORAGE_KEYS.ROLES);
        localStorage.removeItem(window.authManager.STORAGE_KEYS.DEVELOPER);

        // sessionStorage 정리
        sessionStorage.clear();

        // 타이머 정리
        if (window.authManager.sessionTimeoutId) {
            clearTimeout(window.authManager.sessionTimeoutId);
            window.authManager.sessionTimeoutId = null;
        }

        // 로그인 페이지로 리다이렉트 (현재 페이지가 로그인 페이지가 아닌 경우)
        if (!window.location.pathname.includes('/login')) {
            window.location.href = '/login';
        }
    },

    // 세션 타임아웃 리셋
    resetSessionTimeout: () => {
        window.authManager.lastActivityTime = Date.now();

        // 기존 타이머 클리어
        if (window.authManager.sessionTimeoutId) {
            clearTimeout(window.authManager.sessionTimeoutId);
        }

        // 새로운 타이머 설정 (인증된 사용자에게만)
        if (window.authManager.isAuthenticated()) {
            window.authManager.sessionTimeoutId = setTimeout(() => {
                window.authManager.logout('session_timeout');
            }, window.authManager.SESSION_TIMEOUT);
        }
    },

    // 브라우저 종료 감지 및 처리
    setupBrowserCloseDetection: () => {
        // beforeunload 이벤트: 브라우저 종료, 탭 닫기, 새로고침 등
        window.addEventListener('beforeunload', (event) => {
            if (window.authManager.isAuthenticated()) {
                // 세션 정보 저장 (브라우저 종료 vs 새로고침 구분용)
                sessionStorage.setItem('lastActiveTime', Date.now().toString());
                sessionStorage.setItem('isClosing', 'true');

                // 짧은 지연 후 플래그 제거 (새로고침인 경우)
                setTimeout(() => {
                    sessionStorage.removeItem('isClosing');
                }, 100);
            }
        });

        // pagehide 이벤트: 페이지가 숨겨질 때 (더 안정적)
        window.addEventListener('pagehide', (event) => {
            if (event.persisted) {
                // 페이지가 bfcache에 저장됨 (뒤로가기 등)
                sessionStorage.setItem('pageInCache', 'true');
            } else {
                // 페이지가 완전히 언로드됨
                if (window.authManager.isAuthenticated()) {
                    sessionStorage.setItem('pageUnloaded', 'true');
                }
            }
        });

        // pageshow 이벤트: 페이지가 표시될 때
        window.addEventListener('pageshow', (event) => {
            if (event.persisted) {
                // bfcache에서 복원됨
                sessionStorage.removeItem('pageInCache');
            }
        });
    },

    // 페이지 로드 시 브라우저 종료 확인
    checkBrowserClosure: () => {
        const wasClosing = sessionStorage.getItem('isClosing');
        const pageUnloaded = sessionStorage.getItem('pageUnloaded');
        const lastActiveTime = sessionStorage.getItem('lastActiveTime');

        if (pageUnloaded && !wasClosing) {
            // 브라우저가 완전히 종료되었다가 다시 시작됨
            console.log('Browser was completely closed, clearing authentication');
            window.authManager.logout('browser_closed');
            return;
        }

        if (lastActiveTime) {
            const timeDiff = Date.now() - parseInt(lastActiveTime);
            // 1시간 이상 지났으면 자동 로그아웃
            if (timeDiff > 60 * 60 * 1000) {
                console.log('Too much time passed since last activity, logging out');
                window.authManager.logout('time_expired');
                return;
            }
        }

        // cleanup
        sessionStorage.removeItem('isClosing');
        sessionStorage.removeItem('pageUnloaded');
        sessionStorage.removeItem('lastActiveTime');
    },

    // 사용자 활동 추적 설정
    setupActivityTracking: () => {
        // 사용자 활동을 감지하는 이벤트들
        const activityEvents = [
            'mousedown', 'mousemove', 'keypress', 'scroll',
            'touchstart', 'click', 'keydown', 'keyup'
        ];

        // 각 이벤트에 대해 리스너 등록
        activityEvents.forEach(eventName => {
            document.addEventListener(eventName, () => {
                window.authManager.resetSessionTimeout();
            }, true);
        });

        // 페이지 가시성 변경 감지
        document.addEventListener('visibilitychange', () => {
            if (!document.hidden) {
                // 탭이 다시 활성화됨
                window.authManager.resetSessionTimeout();

                // 오랜 시간 비활성화 후 활성화된 경우 인증 상태 재확인
                const timeSinceLastActivity = Date.now() - window.authManager.lastActivityTime;
                if (timeSinceLastActivity > window.authManager.SESSION_TIMEOUT) {
                    window.authManager.logout('inactivity_timeout');
                }
            }
        });
    },

    // 초기화 함수
    initialize: () => {
        console.log('Initializing auth manager...');

        // 브라우저 종료 감지 설정
        window.authManager.setupBrowserCloseDetection();

        // 페이지 로드 시 브라우저 종료 확인
        window.authManager.checkBrowserClosure();

        // 사용자 활동 추적 설정
        window.authManager.setupActivityTracking();

        // 인증된 사용자인 경우 세션 타임아웃 시작
        if (window.authManager.isAuthenticated()) {
            window.authManager.resetSessionTimeout();
        }

        console.log('Auth manager initialized');
    }
    };

    _auth._nexacrm_init = true;
    window.authManager = Object.assign(window.authManager || {}, _auth);

    // DOM 로드 완료 후 초기화
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', window.authManager.initialize);
    } else {
        window.authManager.initialize();
    }
})();
