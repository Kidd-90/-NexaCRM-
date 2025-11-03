# NexaCRM CSS 아키텍처

## 📁 CSS 파일 구조

```
NexaCRM.UI/
├── wwwroot/
│   └── css/
│       ├── app.css                          # 🌐 전역 디자인 시스템
│       ├── loading.css                      # ⏳ 로딩 애니메이션
│       ├── ui/                              # 🧱 모듈형 스타일 계층(토큰/유틸리티/컴포넌트/레이아웃/패턴)
│       │   ├── index.css                    # 📦 ui 하위 파일 번들 엔트리 포인트
│       │   ├── foundations.css              # 🎨 디자인 토큰/커스텀 프로퍼티
│       │   ├── utilities.css                # 🛠️ 단일 책임 유틸리티 클래스
│       │   ├── components.css               # 🧩 공통 컴포넌트 패턴
│       │   ├── layout.css                   # 🗂️ 레이아웃/그리드 패턴
│       │   └── patterns.css                 # 🔁 반복되는 페이지 반응형 패턴
│       └── components/                      # 🧩 재사용 가능한 컴포넌트 스타일
│           ├── table-responsive.css         # 📊 반응형 테이블/카드
│           └── login-status.css             # 🟢 로그인 상태 인디케이터
│
└── Shared/
    ├── NavigationTail.razor                 # 🧭 네비게이션 컴포넌트
    └── NavigationTail.razor.css             # 🎨 NavigationTail 전용 스타일 (Scoped CSS)
```

## 🎯 각 파일의 역할

### **app.css** - 전역 디자인 시스템
**포함 내용**:
- ✅ CSS 변수 (`:root` - 색상, 타이포그래피, 간격 등)
- ✅ 글로벌 타이포그래피 설정
- ✅ 다크/라이트 테마 시스템
- ✅ 공통 레이아웃 (`.page`, `.sidebar`, `.layout-content-container`)
- ✅ 전역 애니메이션 (@keyframes)
- ✅ 유틸리티 클래스 (`.bg-*`, `.text-*`, `.border-*`)

**제외 내용**:
- ❌ 컴포넌트별 특화 스타일
- ❌ 페이지별 특화 스타일

### **ui/index.css** - 모듈형 번들 엔트리
**구성 파일**:
- `foundations.css`: 디자인 토큰, CSS 변수, 테마 프리셋
- `utilities.css`: `ui-min-h-*`, `ui-gap-*` 등 속성 기반 유틸리티
- `components.css`: 카드, 툴바, 배지 등 재사용 컴포넌트
- `layout.css`: 콘텐츠 래퍼, 그리드, 플렉스 헬퍼
- `patterns.css`: 대시보드 레이아웃, 공통 반응형 규칙(사이드바 숨김, 카드 스택 등)

> 페이지 전용 scoped CSS가 아닌, 여러 화면에서 재사용되는 패턴은 `patterns.css`로 승격해 관리하세요.

### **components/table-responsive.css** - 반응형 테이블
**사용 컴포넌트**:
- CustomerManagementPage
- 기타 리스트 페이지

**제공 클래스**:
```css
.desktop-table-view  /* 데스크톱: 테이블 표시 */
.mobile-card-view    /* 모바일: 카드 표시 */
.customer-card       /* 카드 스타일 */
```

### **components/login-status.css** - 로그인 상태 인디케이터
**제공 클래스**:
```css
.login-status-indicator       /* 우측 하단 고정 인디케이터 */
.indicator-dot                /* 녹색 상태 점 */
.indicator-text               /* 상태 텍스트 */
```

### **NavigationTail.razor.css** - NavigationTail 전용 (Scoped CSS)
**Blazor Scoped CSS 특징**:
- 자동으로 고유 속성 추가 (예: `[b-hqrahhuzx2]`)
- 스타일 격리 - 다른 컴포넌트에 영향 없음
- 빌드 시 `NexaCRM.UI.bundle.scp.css`로 번들링

**제공 스타일**:
- `.nav-rail__*` - NavigationTail 레이아웃
- `.rail-icon` - 아이콘 버튼
- `.rail-panel-item` - 패널 메뉴 아이템
- `.rail-panel-item.active` - 활성 메뉴 아이템 (흰색 테두리 + 그라데이션)

## 📌 사용 가이드

### 1️⃣ 새 페이지 생성 시
```razor
@page "/my-page"

<!-- 필요한 컴포넌트 CSS만 선택적으로 포함 -->
<link href="_content/NexaCRM.UI/css/components/table-responsive.css" rel="stylesheet" />

<!-- 페이지 전용 스타일은 scoped CSS 사용 -->
```

**페이지 전용 scoped CSS 생성**:
```
MyPage.razor
MyPage.razor.css  ← 이 파일에 페이지별 스타일 작성
```

### 2️⃣ 새 컴포넌트 생성 시

**옵션 A: Scoped CSS (권장)**
```
MyComponent.razor
MyComponent.razor.css  ← 컴포넌트 전용 스타일
```

**옵션 B: 재사용 가능한 컴포넌트 CSS**
```
wwwroot/css/components/my-component.css
```
→ 여러 페이지에서 사용할 경우

### 3️⃣ 공통 스타일 추가 시

**전역 CSS 변수/유틸리티** → `app.css`에 추가
```css
:root {
    --my-new-color: #FF5733;
}

.my-utility-class {
    /* 전역 유틸리티 */
}
```

**공통 패턴(여러 페이지에서 재사용)** → `wwwroot/css/ui/patterns.css`

**컴포넌트 특화** → 해당 컴포넌트 `.razor.css`에 추가

## 🎨 Scoped CSS vs 글로벌 CSS

### Scoped CSS 사용 시기 ✅
- 컴포넌트 전용 스타일
- 다른 컴포넌트와 스타일 충돌 방지 필요
- 컴포넌트 재사용성 중요

**장점**:
- 스타일 격리
- 클래스 이름 충돌 없음
- 컴포넌트와 스타일이 함께 관리됨

### 글로벌 CSS 사용 시기 ✅
- 디자인 시스템 토큰 (변수, 색상, 타이포그래피)
- 여러 페이지에서 공통 사용하는 레이아웃
- 전역 애니메이션
- 유틸리티 클래스

## 🔧 빌드 시 생성되는 파일

### WebClient (Blazor WebAssembly)
```
publish/wwwroot/_content/NexaCRM.UI/
├── css/
│   ├── app.css
│   └── loading.css
├── NexaCRM.UI.bundle.scp.css  ← 모든 scoped CSS 번들
└── ...
```

### WebServer (Blazor Server)
런타임에 `_content/NexaCRM.UI/` 경로로 정적 파일 제공

## ✨ Active Item 스타일 적용 예제

NavigationTail의 active 메뉴 아이템은 자동으로 다음 스타일이 적용됩니다:

```css
/* NavigationTail.razor.css (Scoped) */
.nav-rail__panel .rail-panel-item.active {
    color: #000000 !important;
    background: linear-gradient(135deg, rgba(245, 245, 245, 0.95) 0%, rgba(255, 255, 255, 0.98) 100%) !important;
    border: 2px solid rgba(255, 255, 255, 0.9) !important;
    border-left: 3px solid #000000 !important;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1), 0 4px 16px rgba(0, 0, 0, 0.08), inset 0 1px 2px rgba(255, 255, 255, 0.6) !important;
}
```

결과:
- ⚪ 흰색 테두리
- ✨ 회색-흰색 그라데이션
- ⬛ 왼쪽 검은색 3px 세로선
- 🌟 부드러운 그림자

## 📚 참고

- [Blazor CSS Isolation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation)
- [CSS 변수](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
- [CSS Scoping](https://developer.mozilla.org/en-US/docs/Web/CSS/:scope)

---

**마지막 업데이트**: 2025-10-09
