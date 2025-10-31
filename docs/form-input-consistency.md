# Form Input Consistency Guide

## Overview
- 목적: 로그인 화면에서 정의한 `--touch-target-min` 토큰을 전체 앱의 폼 컨트롤에 확장해 최소 44px 터치 타깃을 보장합니다.
- 범위: Bootstrap 기반 `.form-control`, `.form-select`, `.btn` 변형과 페이지 스코프 필터/툴바에 포함된 모든 상호작용 요소.
- 위치: `src/NexaCRM.UI/wwwroot/css/ui/forms.css`

## Token Scale
| Token | Description | Default Value |
| --- | --- | --- |
| `--ui-control-height` | 기본 입력/버튼 높이. 로그인 토큰(`--touch-target-min`)을 그대로 사용합니다. | `var(--touch-target-min, 44px)` |
| `--ui-control-height-lg` | 강조 컨트롤 높이. 기본 높이보다 8px 크게 조정해 CTA 버튼이나 주요 폼에 사용합니다. | `clamp(52px, calc(var(--touch-target-min, 44px) + 8px), 60px)` |
| `--ui-control-height-sm` | 스몰 사이즈도 44px 최소값을 유지하도록 기본 높이로 고정합니다. | `var(--ui-control-height)` |
| `--ui-control-padding-y` | 기본 컨트롤 상하 패딩. 뷰포트에 따라 부드럽게 스케일링됩니다. | `clamp(0.65rem, 0.55rem + 0.3vw, 0.8rem)` |
| `--ui-control-padding-y-lg` | 대형 컨트롤 상하 패딩. | `clamp(0.75rem, 0.65rem + 0.35vw, 0.95rem)` |
| `--ui-control-padding-y-sm` | 소형 컨트롤 상하 패딩. | `clamp(0.55rem, 0.5rem + 0.25vw, 0.7rem)` |

## Implementation Notes
1. **Global Import**: `ui/index.css`가 `forms.css`를 포함하므로 별도 페이지에서 추가 import가 필요하지 않습니다.
2. **Bootstrap Compatibility**: `:where(.form-control, .form-select, .btn)` 계열 선택자를 사용해 기존 Bootstrap 스타일 우선순위를 깨지 않고 높이만 재정의합니다.
3. **Size Variants**: `.btn-sm`, `.form-control-sm` 등 크기 변형도 `min-height`를 44px 이상으로 유지하며, 대신 가로 패딩과 폰트 크기로 차별화합니다.
4. **Utility Alignment**: `.ui-input-height` 계열 유틸리티 역시 토큰을 참조하도록 갱신해 커스텀 폼에서도 일관된 높이를 유지합니다.
5. **Page Overrides**: Biz/DB 관리 페이지 필터는 폭(`width: 100%`)만 지정하면 되며, 별도 높이 선언이 필요 없습니다.
6. **Dashboard Search Adoption**: `Pages/MainDashboard`의 글로벌 검색 입력을 `.form-control`로 전환해 공유 스케일을 검증했으니, 다른 대시보드 검색 필드도 동일한 패턴을 재사용하세요.

## QA Checklist
- 360px/768px/1280px 폭에서 필터와 툴바 버튼이 44px 이상인지 DevTools에서 확인합니다.
- 스크린 리더 모드에서 포커스 링이 컨트롤 영역 밖으로 벗어나지 않는지 확인합니다.
- `prefers-reduced-motion` 환경에서도 버튼 높이가 유지되는지 확인합니다.
- Icon-only 버튼(`.btn` + `<i>`)은 `display: inline-flex`를 통해 세로 정렬이 깨지지 않는지 확인합니다.

## Testing
- `dotnet build NexaCrmSolution.sln --configuration Release`
- `dotnet test tests/NexaCRM.UI.Tests/NexaCRM.UI.Tests.csproj --configuration Release`
  - 로컬 환경에 .NET 8 SDK가 없으면 CI에서 실행하세요.
