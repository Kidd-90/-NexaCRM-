# 알림 설정 DB 연동 구현 완료

## 📋 구현 내용

### 1. **Supabase 모델 추가**
파일: `/src/NexaCRM.Service/Abstractions/Models/Supabase/NotificationSettingsRecord.cs`

- `notification_settings` 테이블과 매핑되는 C# 모델
- 모든 알림 설정 필드 포함
- 기본값: 대부분 `true`, Push 알림만 `false`

### 2. **NotificationService 개선**
파일: `/src/NexaCRM.Service/Core/Admin/Services/NotificationService.cs`

#### 주요 기능:
- ✅ **DB에서 설정 로드**: 사용자별 알림 설정 조회
- ✅ **자동 기본값 생성**: DB에 설정이 없으면 기본값으로 자동 생성
- ✅ **설정 저장**: Upsert 로직 (있으면 업데이트, 없으면 삽입)
- ✅ **인증 통합**: AuthenticationStateProvider를 통한 사용자 ID 추출
- ✅ **에러 처리**: 모든 작업에 try-catch 및 로깅 추가

#### 기본 설정값:
```csharp
NewLeadCreated = true
LeadStatusUpdated = true
DealStageChanged = true
DealValueUpdated = true
NewTaskAssigned = true
TaskDueDateReminder = true
EmailNotifications = true
InAppNotifications = true
PushNotifications = false  // 유일하게 false
```

### 3. **NotificationSettingsPage UI 개선**
파일: `/src/NexaCRM.UI/Pages/NotificationSettingsPage.razor`

#### 추가된 기능:
- ⏳ **로딩 상태**: 설정 로드 중 표시
- 💾 **저장 중 상태**: 저장 버튼 비활성화 및 "저장 중..." 표시
- ✅ **성공 메시지**: 저장 완료 시 3초간 표시 후 자동 제거
- ❌ **에러 메시지**: 로드/저장 실패 시 사용자 친화적 메시지 표시
- 🔒 **버튼 비활성화**: 로딩/저장 중 버튼 비활성화

### 4. **샘플 데이터 SQL**
파일: `/supabase/sample-data/notification_settings_samples.sql`

- 사용자 설정 조회 쿼리
- 수동 데이터 생성 쿼리 (주석 처리됨)
- 설정 업데이트 쿼리
- 전체 사용자 설정 조회 쿼리

## 🔄 작동 흐름

### 최초 접속 시:
1. 사용자가 `/notification-settings-page` 접속
2. `GetSettingsAsync()` 호출
3. DB에서 사용자 설정 조회
4. **설정이 없으면** → 기본값으로 자동 생성 및 DB 저장
5. **설정이 있으면** → DB 데이터 로드
6. UI에 설정 표시

### 설정 저장 시:
1. 사용자가 토글 스위치 변경 후 "저장" 클릭
2. `SaveSettingsAsync()` 호출
3. DB에서 기존 설정 확인
4. **있으면** → UPDATE
5. **없으면** → INSERT
6. 성공 메시지 표시 (3초 후 자동 제거)

## 🗄️ 데이터베이스 구조

```sql
notification_settings 테이블:
- user_id (UUID, PRIMARY KEY) - 사용자 ID
- new_lead_created (BOOLEAN)
- lead_status_updated (BOOLEAN)
- deal_stage_changed (BOOLEAN)
- deal_value_updated (BOOLEAN)
- new_task_assigned (BOOLEAN)
- task_due_date_reminder (BOOLEAN)
- email_notifications (BOOLEAN)
- in_app_notifications (BOOLEAN)
- push_notifications (BOOLEAN)
- updated_at (TIMESTAMPTZ) - 마지막 수정 시간
```

## � DI (Dependency Injection) 설정

### NotificationService 등록
- **Scoped**: `SupabaseClientProvider` (Scoped)를 사용하므로 Scoped로 등록 필요
- 파일: `/src/NexaCRM.Service/DependencyInjection/ServiceCollectionExtensions.cs`
- 코드: `services.AddScoped<INotificationService, NotificationService>();`

### 주의사항
- ❌ `AddSingleton`으로 등록 시 오류 발생:
  ```
  Cannot consume scoped service 'SupabaseClientProvider' from singleton 'INotificationService'
  ```
- ✅ `AddScoped`로 등록하여 해결

## �🔐 보안

- RLS (Row Level Security) 활성화됨
- 사용자는 자신의 설정만 조회/수정 가능
- AuthenticationStateProvider를 통한 인증 검증

## 📝 로깅

모든 작업에 상세 로그 추가:
- `[GetSettingsAsync]`: 설정 로드 시작/완료/오류
- `[SaveSettingsAsync]`: 설정 저장 시작/완료/오류
- `[CreateAndSaveDefaultSettingsAsync]`: 기본값 생성
- `[GetCurrentUserIdAsync]`: 사용자 ID 조회

## ✅ 테스트 방법

1. **서버 재시작**:
   ```bash
   cd /Users/imagineiluv/Documents/GitHub/-NexaCRM-/src/NexaCRM.WebServer
   dotnet run
   ```

2. **테스트 시나리오**:
   - ✅ 로그인 후 `/notification-settings-page` 접속
   - ✅ 처음 접속 시 기본값으로 설정 표시 확인
   - ✅ 토글 스위치 변경 후 저장 클릭
   - ✅ "설정이 저장되었습니다" 메시지 확인
   - ✅ 페이지 새로고침 후 저장된 값 유지 확인
   - ✅ 다른 브라우저/기기에서 로그인 시 동일한 설정 표시

3. **DB 확인** (Supabase SQL Editor):
   ```sql
   -- 자신의 설정 확인
   SELECT * FROM notification_settings 
   WHERE user_id = 'your-user-id';
   ```

## 🚀 다음 단계

현재 구현된 기능:
- ✅ DB에서 설정 로드
- ✅ DB에 설정 저장
- ✅ 자동 기본값 생성
- ✅ 에러 처리
- ✅ 로딩/저장 상태 표시

향후 개선 가능:
- 🔔 실제 알림 발송 로직 구현
- 📧 이메일 알림 발송
- 📱 Push 알림 발송
- ⏰ 알림 빈도 설정 (Frequency, Snooze)
- 🔕 알림 일시 정지 기능

## 📌 주의사항

- 서버를 **재시작**해야 변경사항이 적용됩니다
- 로그인한 상태에서만 설정을 저장할 수 있습니다
- DB 연결 실패 시 기본값을 반환합니다 (앱이 중단되지 않음)

## 🎉 완료!

알림 설정이 이제 Supabase DB와 완전히 연동되었습니다!
