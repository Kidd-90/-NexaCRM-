# NexaCRM.Pages Tailwind Integration

## Overview
- `NexaCRM.Pages`는 Razor Class Library로, Blazor Server 및 WebAssembly에서 공유되는 페이지/레이아웃 리소스를 제공합니다.
- Tailwind CSS 4.x 빌드 파이프라인을 추가해 Server, Client, Pages 전체에서 유틸리티 클래스를 일관되게 사용할 수 있습니다.
- `wwwroot/css/app-shell.css`는 Tailwind로 구성한 팔레트 토큰을 기반으로 서버 호스트 전용 앱 쉘 스타일을 제공합니다.
- `wwwroot/js/layout.js`와 `wwwroot/js/actions.js`는 테마 토글 및 UI 상호작용(복사, 다운로드 등)을 위한 공통 모듈입니다.

## NPM Scripts
- `npm run build:css`: `@tailwindcss/cli`를 사용해 `wwwroot/css/tailwind.css`를 입력으로 받아 `wwwroot/css/output.css`를 생성합니다. 프로덕션 번들에 적합하도록 `--minify` 옵션을 적용합니다.
- `npm run watch:css`: 동일한 CLI를 이용해 개발 중 변경 사항을 실시간으로 감지해 CSS를 재생성합니다.

## Tailwind Config Highlights
- `content` 경로에 Pages/Server/Client의 Razor 및 HTML 파일을 추가해, Tailwind가 실제 사용하는 유틸리티 클래스를 정확히 추적하도록 구성했습니다.
- `wwwroot/css/tailwind.css`에서는 `@import "tailwindcss";`를 통해 기본 유틸리티를 로드하고, 재사용 가능한 `.naf-button` 컴포넌트를 순수 CSS로 정의합니다.

## 빌드 후 CSS 소비
- Tailwind 빌드 결과는 `wwwroot/css/output.css`에 저장되며, Razor Class Library 특성상 `_content/NexaCRM.Pages/css/output.css` 경로를 통해 모든 클라이언트에서 접근할 수 있습니다.
- 상대 경로(`../Pages/wwwroot/css/output.css`)가 필요한 시나리오에서는 각 프로젝트의 `.csproj`에 PostBuild `Copy` 타겟을 추가해 정적 폴더로 복사할 수 있습니다.

## 유지 보수 가이드
1. Tailwind 유틸리티를 추가할 때는 반드시 `npm run build:css`를 실행해 `output.css`를 최신 상태로 유지하세요.
2. Tailwind 버전을 올린 경우, `.naf-button`와 같은 커스텀 레이어가 Tailwind CSS 4.x에서 정상적으로 빌드되는지 확인한 뒤 커밋합니다.
3. CI 환경에서는 `npm ci` 후 `npm run build:css`를 실행해 동일한 결과물을 얻을 수 있습니다.
