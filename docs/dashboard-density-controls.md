# Dashboard Density Controls

## 개요
- 테이블 중심 화면에서 여유/컴팩트 두 가지 밀도 모드를 제공해 정보량과 가독성 사이의 균형을 조절합니다.
- `DensityToggle` 컴포넌트와 `data-density` 커스텀 속성을 조합해 UI 전반에 적용되는 CSS 변수를 토글합니다.
- 기본값은 `comfortable`이며, 로컬 스토리지(`nexacrm:ui:density-mode`)에 사용자의 선택을 저장해 페이지 간 일관된 경험을 제공합니다.

## 구조
1. **컴포넌트**: `Components/Controls/DensityToggle`에서 토글 버튼 UI 및 접근성 속성을 관리합니다.
2. **상태 유지**: Biz/DB 관리 페이지는 `IJSRuntime`을 통해 저장된 밀도 값을 읽고 쓰며, 실패 시 안전하게 콘솔 로그만 남깁니다.
3. **토큰**: `wwwroot/css/ui/density.css`가 공용 밀도 변수를 노출하고, 각 페이지의 `.razor.css`에서 이를 소비합니다.

## 스타일 토큰
| 변수 | 여유 모드 | 컴팩트 모드 | 용도 |
| --- | --- | --- | --- |
| `--ui-density-gap-sm` | `0.75rem` | `≈0.5rem` | 헤더/필터/버튼 간격 |
| `--ui-density-gap-md` | `1rem` | `0.75rem` | 컬럼 간 기본 그리드 간격 |
| `--ui-density-table-padding-y` | `0.75rem` | `0.5rem` | 테이블 행 상하 여백 |
| `--ui-density-table-padding-x` | `0.75rem` | `0.5rem` | 테이블 셀 좌우 여백 |
| `--ui-density-font-scale` | `1` | `0.96` | 테이블 텍스트 축소 비율 |

## 접근성 고려 사항
- 토글 버튼은 `role="group"`과 `aria-pressed` 상태를 제공하며, 키보드 포커스 링이 명확하게 표시됩니다.
- `prefers-reduced-motion` 환경에서는 간격/패딩 전환 애니메이션을 비활성화해 어지러움을 방지합니다.
- 밀도 조정 시 버튼 높이는 공유된 `forms.css` 토큰을 재사용하여 최소 44px 터치 타깃을 유지합니다.

## 적용 범위
- `Pages/BizManagementPage`와 `Pages/DbAdvancedManagementPage`는 헤더에 토글을 배치하고, `.advanced-db` 루트 요소에 `data-density` 속성을 설정해 하위 구성 요소를 자동으로 재테마합니다.
- 추가 대시보드에서도 동일한 패턴을 사용하려면 루트 컨테이너에 `data-density` 속성을 적용하고 필요한 스코프 내에서 변수를 소비하면 됩니다.

## 향후 확장 아이디어
- **사용자 선호 동기화**: API 저장소와 연동해 기기 간 밀도 설정을 공유합니다.
- **세분화된 토큰**: 카드, 배지, 리스트 등 추가 요소에 대한 전용 밀도 변수를 도입합니다.
- **Storybook 샘플**: 토글과 테이블이 결합된 스토리를 제공해 QA와 디자인 리뷰를 간소화합니다.
