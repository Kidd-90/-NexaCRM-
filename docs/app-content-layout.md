# Application Content Layout

## 개요
`AppContentLayout`은 로그인 페이지를 제외한 모든 Blazor 페이지에 공통으로 적용되는 기본 콘텐츠 레이아웃입니다. `ResponsivePage`
컴포넌트를 활용해 콘텐츠 영역의 폭, 패딩, 스크롤 동작을 일관되게 유지하며, 기존 `MainLayout`의 헤더/내비게이션과 자연스럽게 결합됩
니다.

## 구성 요소
- **`AppContentLayout`** (`src/NexaCRM.UI/Shared/AppContentLayout.razor`)
  - `MainLayout`을 부모 레이아웃으로 사용합니다.
  - 모든 페이지를 `ResponsivePage`와 `common-page-container` 클래스로 감싸 일관된 여백과 스크롤 동작을 제공합니다.
  - `data-page-type`, `data-page-route` 속성을 노출하여 CSS 또는 Telemetry에서 페이지별 후처리가 가능합니다.
- **`AppContentLayoutConfigurator`** (`src/NexaCRM.UI/Shared/AppContentLayoutConfigurator.razor`)
  - 페이지에서 `LayoutMode`, 추가 클래스, 사용자 정의 데이터 속성 등을 선언형으로 지정할 수 있는 보조 컴포넌트입니다.
- **`AppContentLayoutSettings` / `AppContentLayoutOptions`**
  - 레이아웃에 전달되는 설정 정보를 캡슐화하며, 페이지에서 선언한 옵션이 반영되도록 지원합니다.
- **`ResponsivePage`** (`src/NexaCRM.UI/Shared/ResponsivePage.razor`)
  - 레이아웃 내부에서 콘텐츠 패딩과 최대 폭을 제어하는 기존 컴포넌트입니다.

## 적용 방식
- `src/NexaCRM.UI/Pages/_Imports.razor`에서 `@layout AppContentLayout`을 지정해 로그인/회원가입을 제외한 모든 페이지에 자동 적용되도
록 구성되어 있습니다.
- 로그인 및 회원 가입 화면과 같이 별도의 레이아웃이 필요한 페이지는 기존처럼 `@layout LoginLayout`을 선언하면 됩니다.
- 페이지에서 `ResponsivePage`를 직접 사용할 필요가 없으며, 과거에 사용하던 페이지는 래퍼를 제거해 중복 구조를 줄였습니다.

## 사용 예시
```razor
@page "/reports-page"
@attribute [Authorize(Roles = "Manager,Admin,Developer")]
@inject IReportService ReportService

<AppContentLayoutConfigurator LayoutMode="expanded" data-responsive-grid="" />

<section class="reports-shell" data-col-span="7">
    <!-- 페이지 콘텐츠 -->
</section>
```

## 스타일 가이드
- `MainLayout`의 `desktop-shell__content`는 기존 패딩 값을 유지하며, 실제 콘텐츠 패딩은 `AppContentLayout`이 일관되게 제공합니다.
- 페이지 전용 클래스나 레이아웃 모드는 `AppContentLayoutConfigurator`로 선언적으로 지정할 수 있습니다.
- 불필요한 중첩을 피하기 위해 페이지 루트에는 추가 컨테이너 대신 필요한 섹션만 배치하는 것이 좋습니다.

## 개발 시 참고 사항
1. 새 페이지를 추가하면 자동으로 `AppContentLayout`이 적용됩니다.
2. 콘텐츠 폭이 넓어야 하거나 그리드 모드가 필요한 경우 `<AppContentLayoutConfigurator LayoutMode="expanded" data-responsive-grid="" />`처럼 옵션을 선언하면 됩니다.
3. 빌드 및 테스트 전에는 `dotnet build --configuration Release`와 `dotnet test ./tests/BlazorWebApp.Tests --configuration Release`를 통해 회귀를 방지하세요.
