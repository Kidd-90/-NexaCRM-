# AGENTS.md

## 프로젝트 설명
- 이 프로젝트는 .NET 8 기반 Blazor Web App입니다.
- Blazor WebAssembly 및 Blazor Server 혼합 모델 사용 가능.

## 빌드 지침
- 기본 빌드 명령:  
  `dotnet build --configuration Release`
- Blazor Web App 실행 명령:  
  `dotnet run --project ./src/BlazorWebApp`
- 빌드 대상 프레임워크:  
  `net8.0`
- 필요 SDK:  
  .NET 8 이상

## 테스트 지침
- 테스트 프로젝트 경로: `./tests/BlazorWebApp.Tests`
- 테스트 실행 명령:  
  `dotnet test ./tests/BlazorWebApp.Tests --configuration Release`
- 모든 유닛 테스트가 통과해야 함.

## 코드 스타일/정책
- C# 코드: [Microsoft C# 스타일 가이드](https://learn.microsoft.com/ko-kr/dotnet/csharp/fundamentals/coding-style) 준수.
- 커밋 메시지: Conventional Commits 형식 추천.
- 폴더 구조:
    - `src/`: 앱 코드
    - `tests/`: 테스트 코드
    - `wwwroot/`: 정적 파일

## CI/CD 힌트
- 빌드와 테스트가 성공해야 Pull Request 병합 허용.
- outputs: 빌드 결과와 테스트 통과 여부를 명시.

## 기타
- 필요한 NuGet 패키지 설치 후 빌드 진행(예: `dotnet restore`)
- 환경 변수: 특별한 환경 변수 필요 시, `.env` 또는 `appsettings.json`에 명시

---
