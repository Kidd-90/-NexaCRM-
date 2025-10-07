# Customer Notices 테이블 데이터 접근 문제 해결

## 현재 상황
- ✅ 테이블 생성됨: `customer_notices`
- ✅ 데이터 존재함
- ❌ 애플리케이션에서 데이터를 가져오지 못함

## 원인
**Row Level Security (RLS)** 정책 문제

Supabase는 기본적으로 모든 테이블에 RLS가 활성화되어 있으며, 
정책이 없으면 인증된 사용자라도 데이터를 읽을 수 없습니다.

## 즉시 해결 방법

### Supabase Dashboard에서 실행

1. **Supabase Dashboard 접속**
   - URL: https://vntcbjjuxmifipusovfl.supabase.co
   - 왼쪽 메뉴 > **SQL Editor** 클릭

2. **다음 SQL 실행**:

```sql
-- RLS 비활성화 (개발 환경용)
ALTER TABLE customer_notices DISABLE ROW LEVEL SECURITY;

-- 스키마 캐시 새로고침 (중요!)
NOTIFY pgrst, 'reload schema';

-- 데이터 확인
SELECT COUNT(*) FROM customer_notices;
SELECT id, title, category, importance, is_pinned 
FROM customer_notices 
ORDER BY is_pinned DESC, published_at DESC 
LIMIT 5;
```

3. **브라우저에서 애플리케이션 새로고침**
   - http://localhost:5000
   - 공지사항 페이지로 이동

4. **완료!** 데이터가 보여야 합니다.

---

## 프로덕션 환경용 (권장)

개발이 완료되면 RLS를 다시 활성화하고 적절한 정책을 추가:

```sql
-- RLS 활성화
ALTER TABLE customer_notices ENABLE ROW LEVEL SECURITY;

-- 읽기 정책: 모든 사용자
CREATE POLICY "Anyone can read notices"
ON customer_notices
FOR SELECT
TO public
USING (true);

-- 쓰기 정책: 인증된 사용자만
CREATE POLICY "Authenticated users can insert notices"
ON customer_notices
FOR INSERT
TO authenticated
WITH CHECK (true);

CREATE POLICY "Authenticated users can update notices"
ON customer_notices
FOR UPDATE
TO authenticated
USING (true)
WITH CHECK (true);

CREATE POLICY "Authenticated users can delete notices"
ON customer_notices
FOR DELETE
TO authenticated
USING (true);

-- 스키마 캐시 새로고침
NOTIFY pgrst, 'reload schema';
```

---

## 추가 정보

### 서버 상태
- ✅ NexaCRM.WebServer가 이미 실행 중
- 포트: https://localhost:7065 또는 http://localhost:5000

### 다음 단계
1. 위의 SQL 실행
2. 브라우저 새로고침
3. 공지사항 페이지 확인

문제가 해결되지 않으면 브라우저 개발자 도구 > Console에서 에러 메시지를 확인해주세요.
