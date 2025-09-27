# Blazor Hosting Alignment Guide

## 목적
이 문서는 현재 NexaCRM 솔루션에서 Blazor Server 호스트(`NexaCRM.WebServer`)와 Blazor WebAssembly 호스트(`NexaCRM.WebClient`)가 동일한 UI 구성 요소와 도메인 기능을 공유하기 위해 어떤 구조를 사용하고 있는지, 추가 프로젝트가 필요한지 여부를 정리합니다.

## 프로젝트 역할과 책임

### 공유 UI 계층
- `NexaCRM.UI` 프로젝트는 Razor 컴포넌트, 레이아웃, 리소스를 제공하며 호스트에 중립적인 서비스 계약과 모델은 `NexaCRM.Service`의 `Abstractions/Ui*` 경로로 이동했습니다. 이제 UI 계층은 시각화와 구성 요소 작성에만 집중합니다.
- `NexaCRM.WebClient` 및 `NexaCRM.WebServer` 두 호스팅 프로젝트 모두 `NexaCRM.UI`와 `NexaCRM.Service` 라이브러리를 프로젝트 참조로 연결하여 UI 일관성과 서비스 계약을 동시에 재사용합니다.
- 공통 디자인이나 컴포넌트 로직은 `NexaCRM.UI`에 추가하고, 서비스나 데이터 계약은 `NexaCRM.Service`에 배치하여 호스트 간 일관성을 유지합니다.

### 도메인 서비스 계층
- `NexaCRM.Service/Abstractions`는 관리 콘솔, 중복 검사, 통계 등에서 사용하는 **도메인 계약(인터페이스·모델·검증 속성)** 을 정의합니다.
- `NexaCRM.Service/Core`는 이전에 `NexaCRM.WebClient`에 흩어져 있던 관리용 in-memory/Mock 서비스 구현을 수용하도록 재배치되었습니다. 이를 통해 두 호스트가 동일한 구현을 공유하며, WebAssembly 환경에서도 사용할 수 있는 비동기 루프 기반의 `DuplicateMonitorService` 등을 포함합니다.
- Supabase와 직접 통신하는 인프라 계층은 여전히 호스트 특화 서비스(`NexaCRM.WebClient.Services`)에 남겨 두어 브라우저 API 제약을 분리합니다.

### 호스트 조합 루트
- `NexaCRM.WebClient`는 Blazor WebAssembly 전용 부트스트랩을 담당하며, Supabase 전용 서비스·`CustomAuthStateProvider`·모바일 상호작용 도우미와 같은 **클라이언트 런타임 전용 클래스**만 유지합니다.
- `NexaCRM.WebServer`는 서버 호스트로서 Razor 컴포넌트와 서비스 프로젝트를 참조하여 **동일한 UI/도메인 조합**을 ASP.NET Core 호스팅 모델에서 구성합니다.

## 도메인 계약 공유
- `NexaCRM.Service/Abstractions`는 호스트에 중립적인 모델과 인터페이스를 제공하여 클라이언트와 서버에서 동일한 타입을 사용하게 합니다.
- `NexaCRM.WebClient`와 `NexaCRM.WebServer`는 모두 `NexaCRM.Service`의 `Abstractions`와 `Core`를 참조하여 동일한 도메인 구조와 기본 Mock 구현을 공유합니다.

### `NexaCRM.Model` 분리 필요 여부
- 추가로 `NexaCRM.Model`과 같은 별도 프로젝트를 만들어 데이터를 이중으로 정의할 필요는 없습니다.
- 이미 `NexaCRM.Service/Abstractions/Models` 경로에 Blazor Server와 WebAssembly가 동시에 사용하는 **공식 데이터 계약**(예: `Agent`, `NewUser`)이 정리되어 있습니다.
- 공통 값 객체나 유틸리티 타입이 필요한 경우에는 `NexaCRM.BuildingBlocks` 프로젝트가 기반 타입과 횡단 관심사를 제공합니다.
- 따라서 새로운 모델 전용 프로젝트를 도입하기보다는 `Abstractions` 층에서 모델과 인터페이스를 유지 보수하고, 필요 시 `Core` 구현과 `BuildingBlocks` 유틸리티로 역할을 분리하는 것이 권장됩니다.

## 추가 프로젝트 필요성 평가
- 현재 구조는 "공유 UI 라이브러리 + 다중 호스트" 패턴을 따르므로 Blazor Server와 WebAssembly가 동일한 기능과 디자인을 제공하기 위해 **추가 프로젝트는 필수적이지 않습니다**.
- 이미 다음 두 가정을 만족하므로 호스트별 기능 차이를 최소화할 수 있습니다:
  1. 공통 UI 요소는 `NexaCRM.UI`에 구현한다.
  2. 공통 도메인 계약은 `NexaCRM.Service`의 `Abstractions` 계층을 통해 재사용한다.
- 단, 플랫폼 특화(예: 서버 전용 API 호출, 브라우저 전용 스토리지)가 필요한 경우, `partial` 클래스나 `IPlatformService` 추상화를 `NexaCRM.UI`에 정의하고 호스트별 구현을 각 호스트 프로젝트에 배치하면 됩니다.

## 권장 운영 가이드
1. **컴포넌트 추가 시**: `NexaCRM.UI/Components` 경로 아래에 컴포넌트를 작성하고, 필요한 서비스 인터페이스는 `NexaCRM.Service/Abstractions/UiServices`에 추가합니다.
2. **플랫폼 조건부 로직**: `#if BlazorWebAssembly` 같은 컴파일 상수를 사용하기보다 DI를 통한 추상화 구현 교체로 호스트별 차이를 관리합니다.
3. **리소스 관리**: 다국어 리소스나 스타일 시트는 `NexaCRM.UI/Resources` 및 `wwwroot` 폴더에 유지하여 두 호스트가 공유하도록 합니다.
4. **테스트**: UI 컴포넌트는 [bUnit](https://bunit.dev/)과 같은 테스트 프레임워크로 단위 테스트를 작성하고, 도메인 계약은 xUnit 기반 테스트 프로젝트에서 검증합니다.

## 결론
- 현재 솔루션 구조만으로도 Blazor Server와 Blazor WebAssembly 간 기능 및 UI 동기화를 유지할 수 있습니다.
- 별도의 추가 프로젝트는 필요하지 않으며, 공통 UI/도메인 라이브러리를 지속적으로 관리하는 것이 핵심입니다.
