# Customer Notices 데이터 가져오기 문제 진단 가이드

## 문제 증상
`SupabaseNoticeService`에서 `customer_notices` 테이블 데이터를 가져오지 못함

## 가능한 원인들

### 1. 테이블이 실제로 생성되지 않음
**확인 방법:**
```sql
-- Supabase Dashboard > SQL Editor에서 실행
SELECT EXISTS (
  SELECT FROM information_schema.tables 
  WHERE table_schema = 'public' 
  AND table_name = 'customer_notices'
);
```

**해결책:** `schema.sql`을 실행하여 테이블 생성
```bash
# Supabase Dashboard에서 schema.sql의 customer_notices 부분 실행
```

---

### 2. RLS (Row Level Security) 정책 문제
Supabase는 기본적으로 모든 테이블에 RLS가 활성화되어 있고, 정책이 없으면 데이터를 읽을 수 없습니다.

**확인 방법:**
```sql
-- RLS가 활성화되어 있는지 확인
SELECT rowsecurity FROM pg_tables WHERE tablename = 'customer_notices';

-- 현재 정책 확인
SELECT * FROM pg_policies WHERE tablename = 'customer_notices';
```

**해결책:** RLS 정책 추가 또는 비활성화

#### 옵션 A: RLS 비활성화 (개발/테스트 환경용)
```sql
ALTER TABLE customer_notices DISABLE ROW LEVEL SECURITY;
```

#### 옵션 B: RLS 정책 추가 (프로덕션 권장)
```sql
-- 인증된 사용자에게 읽기 권한 부여
CREATE POLICY "Allow authenticated users to read notices"
ON customer_notices
FOR SELECT
TO authenticated
USING (true);

-- 인증된 사용자에게 쓰기 권한 부여
CREATE POLICY "Allow authenticated users to insert notices"
ON customer_notices
FOR INSERT
TO authenticated
WITH CHECK (true);

-- 인증된 사용자에게 업데이트 권한 부여
CREATE POLICY "Allow authenticated users to update notices"
ON customer_notices
FOR UPDATE
TO authenticated
USING (true)
WITH CHECK (true);

-- 인증된 사용자에게 삭제 권한 부여
CREATE POLICY "Allow authenticated users to delete notices"
ON customer_notices
FOR DELETE
TO authenticated
USING (true);
```

---

### 3. 데이터가 없음
**확인 방법:**
```sql
SELECT COUNT(*) FROM customer_notices;
```

**해결책:** 샘플 데이터 삽입
```bash
# 제공된 스크립트 실행
supabase/migrations/insert_customer_notices_sample_data.sql
```

---

### 4. Supabase 클라이언트 초기화 문제
**확인 사항:**
- Supabase URL이 올바르게 설정되어 있는지
- Supabase API Key가 올바른지
- 네트워크 연결이 정상인지

**해결책:** 환경 변수 확인
```bash
# appsettings.json 또는 환경 변수 확인
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "Key": "your-anon-key"
  }
}
```

---

### 5. 스키마 캐시 문제
PostgREST는 스키마를 캐시하므로, 새로 만든 테이블이 즉시 인식되지 않을 수 있습니다.

**해결책:**
```sql
-- PostgREST 스키마 캐시 새로고침
NOTIFY pgrst, 'reload schema';
```

또는 Supabase Dashboard에서:
- Settings > API > "Reload schema cache" 버튼 클릭

---

## 단계별 문제 해결

### Step 1: 테이블 존재 확인
```sql
-- /supabase/test_customer_notices.sql 실행
```

### Step 2: RLS 정책 적용
```sql
-- 개발 환경이라면 RLS 비활성화
ALTER TABLE customer_notices DISABLE ROW LEVEL SECURITY;
```

### Step 3: 샘플 데이터 삽입
```sql
-- /supabase/migrations/insert_customer_notices_sample_data.sql 실행
```

### Step 4: 스키마 캐시 새로고침
```sql
NOTIFY pgrst, 'reload schema';
```

### Step 5: 애플리케이션 재시작
- 웹 서버 재시작
- 브라우저 캐시 클리어

---

## 개선된 디버깅

`SupabaseNoticeService.cs`에 추가된 로그를 통해 더 자세한 정보를 확인할 수 있습니다:

```csharp
// 이제 다음과 같은 로그가 출력됩니다:
// - "Starting to fetch notices from Supabase..."
// - "Supabase client obtained successfully."
// - "Successfully fetched {Count} notices from Supabase."
// 또는 에러 발생 시:
// - "Failed to load notices from Supabase. Error type: {ErrorType}, Message: {Message}"
```

---

## 빠른 해결 스크립트

아래 SQL을 Supabase Dashboard의 SQL Editor에서 한 번에 실행:

```sql
-- 1. RLS 비활성화 (개발 환경용)
ALTER TABLE customer_notices DISABLE ROW LEVEL SECURITY;

-- 2. 스키마 캐시 새로고침
NOTIFY pgrst, 'reload schema';

-- 3. 데이터 확인
SELECT COUNT(*) FROM customer_notices;

-- 4. 데이터가 없다면 샘플 데이터 삽입 (insert_customer_notices_sample_data.sql 참조)
```

---

## 추가 도움

문제가 계속된다면:
1. 브라우저 개발자 도구에서 네트워크 탭 확인
2. Supabase Dashboard > Logs 확인
3. 애플리케이션 로그에서 상세 에러 메시지 확인
