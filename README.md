# -NexaCRM-
NexaCRM은 고객 관리부터 워크플로우 자동화, 보고서까지 한 번에 처리할 수 있는 종합 CRM 솔루션입니다. 반응형 UI와 메시지 브로커 기반 백엔드를 통해 유연한 확장과 안정적인 운영을 지원합니다.

📝 개요
NexaCRM은 고객 관리, 리드 관리, 영업 파이프라인, 워크플로우 자동화, 보고서 생성, 보안 인증을 모두 지원하는 올인원 CRM 플랫폼입니다.
C# Blazor 프론트엔드와 ASP.NET Core 마이크로서비스 백엔드를 이용해, 일관된 코드 베이스로 빠른 개발과 확장을 보장합니다.

🚀 주요 기능
📇 연락처 관리: 360° 고객 뷰 및 상호작용 타임라인

💼 리드 관리: 다채널 리드 스코어링 및 일괄 작업

🏷️ 영업 파이프라인: 칸반 보드, 드래그앤드롭 단계 관리

🤖 워크플로우 자동화: 트리거 기반 시나리오 디자이너

📊 보고서 & 분석: 차트·테이블 커스텀 리포트, PDF/CSV 내보내기

🔒 인증 및 보안: OAuth2.0, JWT, RBAC, OWASP Top 10 준수

📱 반응형 디자인: 데스크탑·태블릿·모바일 최적화

🏗️ 아키텍처
text
[ Ocelot Gateway ] 
       ↓
┌──────────────┐   ┌──────────────┐   ┌──────────────┐
│ Contact MS   │   │ Lead MS      │   │ Deal MS      │
│ (PostgreSQL) │   │ (PostgreSQL) │   │ (PostgreSQL) │
└──────────────┘   └──────────────┘   └──────────────┘
       ↓                  ↓                  ↓
   RabbitMQ            RabbitMQ            RabbitMQ
       ↓                  ↓                  ↓
┌─────────────────────────────────────────────────┐
│           Blazor Server / WASM UI             │
└─────────────────────────────────────────────────┘
       ↓                  ↓                  ↓
 Prometheus         Grafana           Jaeger Tracing
🛠️ 기술 스택
영역	기술
프론트엔드	Blazor Server/WebAssembly, MudBlazor, Radzen, Fluxor
백엔드	ASP.NET Core Web API, EF Core, MediatR, AutoMapper, FluentValidation
데이터베이스	PostgreSQL, Redis
메시징	RabbitMQ, Apache Kafka
인증/보안	Duende IdentityServer, JWT Bearer
인프라	Docker, Kubernetes (AKS/EKS/GKE)
CI/CD	GitHub Actions, Docker Hub / ACR
테스트	xUnit, Moq, Playwright, Selenium
🏁 시작하기
필수 요소
.NET SDK 8.0

Docker & Docker Compose

Node.js (Playwright용)

(선택) PostgreSQL, Redis — Docker로 간편 설치 가능

클론 및 설정
bash
git clone https://github.com/your-org/nexacrm.git
cd nexacrm
로컬 실행
데이터베이스 & 메시지 브로커 실행

bash
docker-compose up -d
마이그레이션 적용

bash
cd src/CrmApi
dotnet ef database update
백엔드 실행

bash
dotnet run --project src/CrmApi/CrmApi.csproj
프론트엔드 실행

bash
dotnet run --project src/CrmUI/CrmUI.csproj
접속

UI: http://localhost:5000

Swagger: http://localhost:5001/swagger

📁 폴더 구조
text
.
├── src
│   ├── CrmApi        # ASP.NET Core 마이크로서비스
│   ├── CrmUI         # Blazor 프론트엔드
│   ├── CrmAuth       # IdentityServer 인증 서비스
│   └── Shared        # 공용 DTO, 유틸리티
├── docs              # 아키텍처 다이어그램, 와이어프레임
├── tests             # 단위·통합·E2E 테스트
└── docker-compose.yml
☁️ 배포
Docker 이미지 빌드

bash
docker build -t nexacrm-api src/CrmApi
레지스트리에 푸시

bash
docker push your-registry/nexacrm-api:latest
Kubernetes 적용

bash
kubectl apply -f k8s/
🤝 컨트리뷰션
포크(Fork)

기능 브랜치 생성(git checkout -b feature/your-feature)

커밋 & 푸시

풀 리퀘스트(PR) 생성

📄 라이선스
MIT © NexaCRM 팀
