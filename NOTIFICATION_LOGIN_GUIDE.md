# 알림 페이지 로그인 문제 해결 가이드

## 🔴 문제 증상
```
[GetAsync] No authenticated Supabase user available when loading notification feed; returning empty feed.
```

## 🔍 원인
Supabase 인증 상태가 없어서 user_id를 가져올 수 없습니다.

## ✅ 해결 방법

### 1. 로그인 상태 확인

**브라우저에서 확인:**
1. F12 개발자 도구 열기
2. **Application** 탭 → **Local Storage** → `http://localhost:XXXX`
3. `supabase.auth.token` 키가 있는지 확인

**예상 결과:**
- ✅ 있음: 로그인 되어 있음
- ❌ 없음: 로그인 필요

### 2. 로그인 페이지로 이동

```
http://localhost:7065/login
또는
http://localhost:5065/login
```

**테스트 계정으로 로그인:**
- Supabase Dashboard → Authentication → Users에서 생성한 계정 사용

### 3. 로그인 후 알림 페이지 재접속

```
http://localhost:7065/notifications
```

### 4. 로그 확인

**예상 정상 로그:**
```
[GetAsync] Starting to load notification feed...
[GetAsync] Realtime subscription ensured.
[GetAsync] Supabase client obtained.
[GetAsync] User ID obtained: {guid}  ← 이 로그가 나와야 함!
[GetAsync] Executing query: Filter by UserId={guid}
[GetAsync] Retrieved X records from database.
```

---

## 🛠️ 대안: 개발 환경에서 임시 사용자 ID 사용

개발/테스트 목적으로 로그인 없이 테스트하려면 코드를 임시로 수정할 수 있습니다.

### 옵션 A: Mock 사용자 반환 (개발 환경 전용)

**SupabaseNotificationFeedService.cs 수정:**

```csharp
private bool TryEnsureUserId(global::Supabase.Client client, out Guid userId)
{
    // 개발 환경에서 임시로 첫 번째 사용자 사용
    #if DEBUG
    var envUserId = Environment.GetEnvironmentVariable("DEV_USER_ID");
    if (!string.IsNullOrEmpty(envUserId) && Guid.TryParse(envUserId, out var devUserId))
    {
        _logger.LogWarning("[DEV MODE] Using development user ID: {UserId}", devUserId);
        userId = devUserId;
        return true;
    }
    #endif

    userId = Guid.Empty;
    var rawId = client?.Auth?.CurrentUser?.Id;
    if (string.IsNullOrWhiteSpace(rawId) || !Guid.TryParse(rawId, out var parsed))
    {
        return false;
    }

    if (!_userId.HasValue || _userId.Value != parsed)
    {
        _userId = parsed;
    }

    userId = _userId.Value;
    return true;
}
```

**환경 변수 설정 (launchSettings.json):**

```json
{
  "profiles": {
    "NexaCRM.WebServer": {
      "environmentVariables": {
        "DEV_USER_ID": "YOUR-USER-ID-FROM-SUPABASE"
      }
    }
  }
}
```

### 옵션 B: 인증 없이 모든 알림 표시 (개발 환경 전용)

**⚠️ 주의: 보안상 개발 환경에서만 사용!**

RLS 정책을 임시로 비활성화하고 모든 알림을 가져오도록 수정할 수 있지만, **프로덕션에서는 절대 사용하면 안 됩니다.**

---

## 🎯 권장 해결 순서

1. ✅ **로그인 확인** (가장 간단하고 안전)
2. ✅ Supabase에 테스트 사용자 생성
3. ✅ `/login` 페이지에서 로그인
4. ✅ `/notifications` 페이지 재접속
5. ✅ 로그에서 `User ID obtained:` 확인

---

## 🔧 Supabase 사용자 생성 방법

### 방법 1: Supabase Dashboard

1. Supabase Dashboard 접속
2. **Authentication** → **Users** 클릭
3. **Add user** 버튼 클릭
4. Email과 Password 입력
5. **Create user** 클릭

### 방법 2: SQL로 생성

```sql
-- 테스트 사용자 생성
INSERT INTO auth.users (
  instance_id,
  id,
  aud,
  role,
  email,
  encrypted_password,
  email_confirmed_at,
  created_at,
  updated_at
)
VALUES (
  '00000000-0000-0000-0000-000000000000',
  gen_random_uuid(),
  'authenticated',
  'authenticated',
  'test@example.com',
  crypt('password123', gen_salt('bf')),
  NOW(),
  NOW(),
  NOW()
)
RETURNING id, email;
```

---

## 📊 디버깅 체크리스트

- [ ] 로그인 페이지가 정상 작동하는가?
- [ ] Supabase URL과 Key가 올바르게 설정되어 있는가?
- [ ] 브라우저 Local Storage에 `supabase.auth.token`이 있는가?
- [ ] 로그인 후 다른 페이지에서 사용자 정보가 보이는가?
- [ ] Supabase RLS 정책이 올바르게 설정되어 있는가?

---

## 🚨 자주 발생하는 문제

### 문제 1: 로그인은 되었는데도 User ID를 못 가져옴

**원인:** Session persistence 문제

**해결:**
1. 브라우저 완전 새로고침 (Ctrl+Shift+R 또는 Cmd+Shift+R)
2. 브라우저 캐시 삭제
3. 로그아웃 후 다시 로그인

### 문제 2: WebClient와 WebServer에서 세션이 공유되지 않음

**원인:** 다른 호스트 모델 사용

**해결:**
- WebClient: `http://localhost:5000` (WASM)
- WebServer: `http://localhost:7065` (Server-side Blazor)
- 각각 별도로 로그인 필요

### 문제 3: Supabase 연결은 되는데 Auth가 null

**원인:** AuthenticationStateProvider 설정 문제

**확인:**
```csharp
// Program.cs 또는 Startup.cs에서
services.AddScoped<AuthenticationStateProvider>(...);
```

---

## ✨ 정상 작동 시 예상 화면

**알림 페이지 (/notifications):**
- 로딩 스피너 → 알림 목록 표시
- "전체 X개" 통계 표시
- 각 알림의 제목, 메시지, 시간 표시

**예상 로그:**
```
info: NexaCRM.Service.Supabase.SupabaseNotificationFeedService[0]
      [GetAsync] User ID obtained: 12345678-1234-1234-1234-123456789abc
info: NexaCRM.Service.Supabase.SupabaseNotificationFeedService[0]
      [GetAsync] Retrieved 5 records from database.
info: NexaCRM.UI.Pages.NotificationsPage[0]
      [NotificationsPage] Successfully loaded 5 notifications.
```
