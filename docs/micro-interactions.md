# Micro-interaction Guidelines

## Overview
- 마이크로 인터랙션은 테이블이 많은 관리자 화면에서 상태 변화를 부드럽게 전달하고, 사용자 집중도를 높이기 위해 도입되었습니다.
- `wwwroot/css/ui/motion.css`는 지속시간·이징 토큰과 공용 유틸리티 클래스를 노출해 페이지마다 별도의 애니메이션 값을 정의하지 않아도 됩니다.
- `prefers-reduced-motion` 사용자는 자동으로 애니메이션이 최소화되므로 감각 민감 사용자도 안전하게 화면을 사용할 수 있습니다.

## Motion Tokens
| 토큰 | 기본값 | 용도 |
| --- | --- | --- |
| `--motion-duration-xs` | `120ms` | 버튼 hover, focus 전환 |
| `--motion-duration-sm` | `180ms` | 탭/토글 상태 변화 |
| `--motion-duration-md` | `240ms` | 섹션 진입, 배너 표시 |
| `--motion-duration-lg` | `360ms` | 모달/툴팁 등장 (필요 시) |
| `--motion-ease-standard` | `cubic-bezier(0.2, 0, 0, 1)` | 기본 완화 이징 |
| `--motion-ease-decelerated` | `cubic-bezier(0, 0, 0.2, 1)` | 진입 애니메이션 감속 |
| `--motion-ease-emphasized` | `cubic-bezier(0.32, 0, 0.67, 0)` | 강조 요소 진입 |

> `prefers-reduced-motion: reduce` 환경에서는 모든 지속시간이 `1ms`로 강제되며 hover/active 변형이 제거됩니다.

## Utility Classes
- `.motion-fade-in`: 투명도만 변하는 짧은 페이드 인 애니메이션.
- `.motion-fade-in-up` / `.motion-fade-in-down`: 12px 이동과 함께 등장하는 섹션/헤더용 애니메이션.
- `.motion-pressable`: hover/focus 시 살짝 상승하는 상호작용 버튼. 기존 박스 섀도우/배경 값과 함께 자연스럽게 전환됩니다.
- `.motion-stagger`: 직계 자식의 `animation-delay`를 60ms 간격으로 증가시켜 리스트/탭에 계단식 등장 효과 부여.

## Implementation Checklist
1. Razor 페이지 또는 컴포넌트 루트에 `motion-fade-in-up`을 부여해 첫 진입 시 부드러운 등장 효과를 제공합니다.
2. 탭, 토글, 주요 액션 버튼에는 `motion-pressable`을 추가해 hover/active 피드백을 통일합니다.
3. 여러 버튼이 동시에 렌더링되는 영역(탭, 툴바 등)은 컨테이너에 `motion-stagger`를 추가하고, 각 항목에 `motion-fade-in`을 결합합니다.
4. 추가 전환이 필요한 경우에는 기존 `transition`을 제거하고 `motion.css` 토큰을 사용해 지속시간/이징을 재정의합니다.
5. `prefers-reduced-motion` 환경을 테스트하기 위해 DevTools나 시스템 설정에서 감속 모드를 활성화하고, 모든 애니메이션이 즉시 종료되는지 확인합니다.

### Razor 예시
```razor
<nav class="tabs motion-stagger" role="tablist">
    <button class="@($"tab motion-pressable motion-fade-in {(isActive ? "active" : string.Empty)}".Trim())">
        세그먼트
    </button>
    <button class="tab motion-pressable motion-fade-in">가져오기</button>
</nav>
```

## QA Notes
- `motion-pressable`을 적용한 버튼은 포커스 링과 충돌하지 않도록 outline이 유지되는지 확인합니다.
- `motion-stagger`는 `display: contents` 요소에는 적용되지 않으므로 탭/리스트 항목을 직접 감싸는 요소가 필요합니다.
- 빌드 전 `dotnet build NexaCrmSolution.sln --configuration Release` 명령으로 Razor Scoped CSS가 정상적으로 컴파일되는지 확인하세요.
