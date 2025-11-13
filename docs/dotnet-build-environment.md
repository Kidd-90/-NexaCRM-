# .NET 8 빌드 환경 구성 가이드

## 개요
- `NexaCrmSolution.sln`은 .NET 8 SDK를 필요로 합니다.
- 루트에 있는 `global.json`은 `8.0.100` 기능 밴드를 기준으로 `rollForward: latestFeature` 옵션을 선언하여 `8.0.4xx`와 같이 더 최신
  패치를 자동으로 활용합니다. CI 이미지에 보다 새로운 8.0 SDK만 설치되어 있어도 `dotnet restore`가 실패하지 않습니다.
- 이 문서는 CI나 로컬 개발 환경에서 `dotnet build --configuration Release`를 실행하기 위한 필수 패키지 설치 절차를 정리합니다.

## 사전 준비
- 지원 OS: Debian 12 (bookworm) 또는 Ubuntu 24.04 (noble) 기준.
- 관리자 권한(`sudo`)이 필요합니다.
- 프록시 환경에서는 `wget`과 `apt`가 외부 저장소에 접근할 수 있도록 설정해야 합니다.

## 설치 절차
1. Microsoft 패키지 저장소 등록
   ```bash
   wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   ```
2. 패키지 인덱스 갱신
   ```bash
   sudo apt-get update
   ```
   - 프록시에서 `mise.jdx.dev` 등의 서드파티 저장소 접근이 차단되는 경우 경고가 나타날 수 있으나, Microsoft 리포지터리만 정상적으로 동작하면 .NET 설치에는 영향이 없습니다.
3. .NET 8 SDK 설치
   ```bash
   sudo apt-get install -y dotnet-sdk-8.0
   ```
4. 설치 검증
   ```bash
   dotnet --version
   # 출력 예: 8.0.414
   ```

## 빌드 및 복원 절차
1. 솔루션 루트(`/workspace/-NexaCRM-`)에서 복원 실행
   ```bash
   dotnet restore
   ```
2. 릴리스 빌드 실행
   ```bash
   dotnet build --configuration Release
   ```
3. 빌드 결과
   - 경고(예: nullable 참조, 비동기 미사용)는 존재하지만, 에러 없이 모든 프로젝트가 성공적으로 빌드됩니다.
   - 빌드 산출물은 각 프로젝트의 `bin/Release/net8.0/` 폴더에 생성됩니다.

## 추가 권장 사항
- nullable 경고를 감소시키려면 서비스 및 Razor 페이지의 null 처리 로직을 점검하세요.
- CI 환경에서는 `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1` 환경 변수를 설정해 초기 인증서 설치 시간을 단축할 수 있습니다.
- 테스트 프로젝트 추가 시 `dotnet test --configuration Release` 명령을 파이프라인에 포함하세요.
