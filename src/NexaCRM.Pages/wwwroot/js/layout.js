// attachMainLayoutJsHandlers: 메인 레이아웃 버튼에 JS 이벤트 연결
export function attachMainLayoutJsHandlers() {
	// 상세 네비 토글
	const detailBtn = document.getElementById('js-detail-nav-btn');
	if (detailBtn) {
		detailBtn.onclick = () => {
			const nav = document.querySelector('.themed-body .flex-1');
			if (nav) {
				nav.classList.toggle('js-detail-nav-open');
			}
		};
	}
	// 테마 토글 (예시: 단순 새로고침)
	const themeBtn = document.getElementById('js-theme-btn');
	if (themeBtn) {
		themeBtn.onclick = () => {
			// 실제 테마 토글 로직은 필요에 따라 구현
			document.body.classList.toggle('dark');
		};
	}
}

const THEME_STORAGE_KEY = 'nexacrm-theme-preference';
let initialized = false;

function resolveStoredTheme() {
	try {
		return window.localStorage.getItem(THEME_STORAGE_KEY) || 'auto';
	} catch {
		return 'auto';
	}
}

function prefersDark() {
	return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
}

function resolveEffectiveTheme() {
	const explicit = document.documentElement.getAttribute('data-theme');
	if (explicit === 'dark' || explicit === 'light') {
		return explicit;
	}

	const stored = resolveStoredTheme();
	if (stored === 'dark' || stored === 'light') {
		return stored;
	}

	return prefersDark() ? 'dark' : 'light';
}

function applyTheme(theme) {
	const value = theme === 'dark' ? 'dark' : 'light';
	document.documentElement.setAttribute('data-theme', value);
	document.documentElement.classList.toggle('dark', value === 'dark');
	try {
		window.localStorage.setItem(THEME_STORAGE_KEY, value);
	} catch {
		// ignore storage errors
	}
}

export function initializeShell() {
	if (initialized) {
		return;
	}

	initialized = true;
	applyTheme(resolveEffectiveTheme());

	if (window.matchMedia) {
		const media = window.matchMedia('(prefers-color-scheme: dark)');
		const handleChange = () => {
			const stored = resolveStoredTheme();
			if (stored === 'auto' || stored === null) {
				applyTheme(resolveEffectiveTheme());
			}
		};

		if (typeof media.addEventListener === 'function') {
			media.addEventListener('change', handleChange);
		} else if (typeof media.addListener === 'function') {
			media.addListener(handleChange);
		}
	}
}

export function isDarkMode() {
	return resolveEffectiveTheme() === 'dark';
}

export function toggleTheme() {
	const next = isDarkMode() ? 'light' : 'dark';
	applyTheme(next);
	return next === 'dark';
}

export default {
	initializeShell,
	isDarkMode,
	toggleTheme,
	attachMainLayoutJsHandlers
};
