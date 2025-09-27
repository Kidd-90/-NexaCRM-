# NexaCRM Frontend Style Sharing

## Overview
- 공통 디자인 토큰과 스타일 자산은 **Razor Class Library**인 `NexaCRM.UI` 프로젝트에서 관리합니다.
- `NexaCRM.WebClient`와 `NexaCRM.WebServer` 모두 `NexaCRM.UI`를 참조하고, `_content/NexaCRM.UI/...` 정적 자산 경로를 통해 동일한 CSS를 로드합니다.

## Included Assets
- `wwwroot/css/app.css`: 데스크톱·다크 모드 테마, 전역 레이아웃, 상단 헤더, 사이드바, 카드 컴포넌트 스타일 정의.
- `wwwroot/css/mobile.css`: 모바일 네비게이션, 반응형 브레이크포인트, 터치 상호작용에 특화된 보조 규칙 제공.
- `wwwroot/js/*.js`: 인증, 내비게이션, 테마, 디바이스 감지 등 공통 상호작용 로직 모음.

## Usage Guidance
1. Razor 컴포넌트 또는 레이아웃에서 다음과 같이 정적 자산을 참조합니다.
   ```html
   <HeadContent>
       <link rel="stylesheet" href="_content/NexaCRM.UI/css/app.css" />
       <link rel="stylesheet" href="_content/NexaCRM.UI/css/mobile.css" />
   </HeadContent>
   ```
2. WebAssembly 호스트(`wwwroot/index.html`)에서도 동일 경로를 사용하면 초기 로딩 스피너까지 일관된 스타일을 유지할 수 있습니다.
3. 공통 스크립트가 필요하면 `<script src="_content/NexaCRM.UI/js/<file-name>.js"></script>` 형태로 포함해 두 애플리케이션에서 동일한 UX 동작을 제공합니다.
4. 새로운 공통 스타일이나 스크립트는 `NexaCRM.UI/wwwroot/` 아래에 추가하고, 두 애플리케이션에서 `_content/NexaCRM.UI/...` 경로로 참조하세요.

## Server Host Integration Checklist
- `src/NexaCRM.WebServer/Pages/_Host.cshtml` 파일에서 WebClient와 동일한 `_content/NexaCRM.UI` CSS, JS 번들을 참조해 서버 렌더링과 WebAssembly 클라이언트 간 스타일 일관성을 확보합니다.
- 초기 오류 배너(`#blazor-error-ui`)와 `window.isDarkMode` 헬퍼를 포함해 공통 테마 전환 로직이 양쪽 호스트 페이지에서 동일하게 동작하도록 유지합니다.
- 변경 후에는 `dotnet build NexaCrmSolution.sln --configuration Release` 명령으로 Razor Class Library 참조가 유효한지 확인합니다.

## Change Management
- 공통 CSS를 수정할 때는 `NexaCRM.UI` 프로젝트의 파일만 변경하고, `dotnet build --configuration Release`로 변경 사항이 정상적으로 적용되는지 확인합니다.
- 스타일 변경 이후에는 WebClient와 WebServer 모두에서 UI 회귀 테스트(수동 또는 자동)를 수행하여 디자인 일관성을 검증하세요.
