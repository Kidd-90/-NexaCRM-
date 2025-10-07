# 알림 페이지 디버깅 가이드

## 문제 상황
- `/notifications` 페이지에서 DB의 알림 데이터를 불러오지 못함
- Supabase의 `notification_feed` 테이블에서 데이터를 가져와야 함

## 체크리스트

### 1. Supabase 연결 확인
```bash
# appsettings.json 또는 환경변수에 Supabase 설정이 있는지 확인
# - SUPABASE_URL
# - SUPABASE_KEY (anon key)
```

### 2. 인증 상태 확인
```bash
# 브라우저 개발자 도구 > Application > Local Storage
# supabase.auth.token이 있는지 확인
```

### 3. 데이터베이스 확인
**Supabase Dashboard에서 확인:**
1. SQL Editor에서 실행:
```sql
-- auth.users 테이블에 사용자가 있는지 확인
SELECT id, email FROM auth.users LIMIT 5;

-- notification_feed 테이블에 데이터가 있는지 확인
SELECT * FROM notification_feed LIMIT 10;

-- 특정 사용자의 알림 확인 (user_id를 실제 값으로 교체)
SELECT * FROM notification_feed 
WHERE user_id = 'YOUR-USER-ID-HERE'
ORDER BY created_at DESC;
```

2. 샘플 데이터 삽입:
```bash
# 생성된 SQL 파일 실행
cd supabase/migrations
# Supabase SQL Editor에서 insert_notification_sample_data.sql 내용 복사 후 실행
```

### 4. RLS (Row Level Security) 정책 확인
```sql
-- RLS가 활성화되어 있는지 확인
SELECT tablename, rowsecurity FROM pg_tables 
WHERE schemaname = 'public' AND tablename = 'notification_feed';

-- RLS 정책 확인
SELECT * FROM pg_policies WHERE tablename = 'notification_feed';
```

### 5. 브라우저 로그 확인
**브라우저 개발자 도구 > Console에서 확인:**
- `[NotificationsPage]` 로그 찾기
- `[GetAsync]` 로그 찾기
- 에러 메시지 확인

**예상 로그 순서:**
```
[NotificationsPage] Loading notifications...
[GetAsync] Starting to load notification feed...
[GetAsync] Realtime subscription ensured.
[GetAsync] Supabase client obtained.
[GetAsync] User ID obtained: {guid}
[GetAsync] Executing query: Filter by UserId={guid}, Order by CreatedAt DESC
[GetAsync] Query executed. Response Models Count: X
[GetAsync] Retrieved X records from database.
[GetAsync] Successfully loaded notification feed with X items.
[NotificationsPage] Successfully loaded X notifications.
```

### 6. 일반적인 문제 및 해결방법

#### 문제 1: "No authenticated Supabase user available"
**원인:** 로그인이 되어 있지 않음
**해결:**
1. `/login` 페이지로 이동
2. 올바른 자격증명으로 로그인
3. 다시 `/notifications` 접근

#### 문제 2: "Query executed. Response Models Count: 0"
**원인:** DB에 해당 사용자의 알림 데이터가 없음
**해결:**
1. Supabase SQL Editor에서 샘플 데이터 삽입
2. `insert_notification_sample_data.sql` 실행
3. 페이지 새로고침

#### 문제 3: RLS 정책으로 인한 접근 거부
**원인:** RLS 정책이 현재 사용자의 접근을 막고 있음
**해결:**
```sql
-- RLS 정책 확인
SELECT * FROM pg_policies WHERE tablename = 'notification_feed';

-- 필요시 임시로 RLS 비활성화 (개발 환경에서만!)
ALTER TABLE notification_feed DISABLE ROW LEVEL SECURITY;

-- 테스트 후 다시 활성화
ALTER TABLE notification_feed ENABLE ROW LEVEL SECURITY;
```

#### 문제 4: Supabase 연결 실패
**원인:** Supabase URL 또는 Key가 잘못됨
**해결:**
1. `appsettings.json` 또는 환경변수 확인
2. Supabase Dashboard > Settings > API에서 올바른 값 복사
3. 서버 재시작

### 7. 수동 테스트 쿼리

**Supabase SQL Editor에서 실행:**

```sql
-- 1. 첫 번째 사용자 ID 가져오기
SELECT id, email FROM auth.users LIMIT 1;

-- 2. 해당 사용자에게 테스트 알림 추가
INSERT INTO notification_feed (user_id, title, message, type, is_read)
VALUES (
  'YOUR-USER-ID-HERE',  -- 위에서 가져온 ID로 교체
  '테스트 알림',
  '이것은 테스트 알림입니다.',
  'info',
  false
);

-- 3. 알림이 정상적으로 삽입되었는지 확인
SELECT * FROM notification_feed 
WHERE user_id = 'YOUR-USER-ID-HERE'
ORDER BY created_at DESC;
```

### 8. 코드 레벨 디버깅

**추가 로그 확인이 필요한 경우:**

1. `SupabaseNotificationFeedService.cs`의 `GetAsync()` 메서드에 이미 상세 로그 추가됨
2. `NotificationsPage.razor`의 `LoadAsync()` 메서드에도 상세 로그 추가됨
3. 브라우저 콘솔에서 로그 확인

**예상 에러 패턴:**

```
❌ User ID가 null인 경우:
[GetAsync] No authenticated Supabase user available

❌ 쿼리 실패:
[GetAsync] Failed to load notification feed from Supabase. Error: {message}

✅ 정상 작동:
[GetAsync] Successfully loaded notification feed with X items.
```

## 빠른 해결 체크리스트

1. ✅ 로그인 되어 있는가?
2. ✅ Supabase 연결 설정이 올바른가?
3. ✅ `notification_feed` 테이블이 존재하는가?
4. ✅ 샘플 데이터가 삽입되어 있는가?
5. ✅ RLS 정책이 현재 사용자를 허용하는가?
6. ✅ 브라우저 콘솔에 에러가 있는가?

## 도움이 되는 명령어

```bash
# 서버 재시작
cd /Users/imagineiluv/Documents/GitHub/-NexaCRM-/src/NexaCRM.WebClient
dotnet run

# 빌드 확인
dotnet build

# 로그 확인 (실행 중인 터미널에서)
# [GetAsync] 또는 [NotificationsPage] 키워드 검색
```

## 참고 파일 위치

- **서비스 구현:** `/src/NexaCRM.Service/Core/Supabase/Services/SupabaseNotificationFeedService.cs`
- **페이지 컴포넌트:** `/src/NexaCRM.UI/Pages/NotificationsPage.razor`
- **DB 모델:** `/src/NexaCRM.Service/Abstractions/Models/Supabase/NotificationFeedRecord.cs`
- **DB 스키마:** `/supabase/migrations/schema.sql` (lines 769-797)
- **RLS 정책:** `/supabase/migrations/rls.sql` (lines 395-405)
- **샘플 데이터:** `/supabase/migrations/insert_notification_sample_data.sql`
