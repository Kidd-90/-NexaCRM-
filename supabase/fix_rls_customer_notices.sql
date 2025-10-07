-- RLS 상태 및 정책 확인 후 해결
-- Supabase Dashboard > SQL Editor에서 실행

-- 1. 현재 RLS 상태 확인
SELECT 
  schemaname,
  tablename,
  rowsecurity AS rls_enabled
FROM pg_tables
WHERE tablename = 'customer_notices';

-- 2. 현재 정책 확인
SELECT 
  policyname,
  permissive,
  roles,
  cmd AS operation,
  qual AS using_expression
FROM pg_policies
WHERE tablename = 'customer_notices';

-- 3. RLS 비활성화 (개발 환경용 - 즉시 해결)
ALTER TABLE customer_notices DISABLE ROW LEVEL SECURITY;

-- 또는

-- 4. RLS 정책 추가 (프로덕션 권장)
-- 기존 정책이 있다면 먼저 삭제
DROP POLICY IF EXISTS "Enable read access for all users" ON customer_notices;
DROP POLICY IF EXISTS "Enable insert for authenticated users only" ON customer_notices;
DROP POLICY IF EXISTS "Enable update for authenticated users only" ON customer_notices;
DROP POLICY IF EXISTS "Enable delete for authenticated users only" ON customer_notices;

-- 새 정책 생성 - 인증된 사용자 모두 접근 가능
CREATE POLICY "Enable read access for all users"
ON customer_notices
FOR SELECT
TO public
USING (true);

CREATE POLICY "Enable insert for authenticated users only"
ON customer_notices
FOR INSERT
TO authenticated
WITH CHECK (true);

CREATE POLICY "Enable update for authenticated users only"
ON customer_notices
FOR UPDATE
TO authenticated
USING (true)
WITH CHECK (true);

CREATE POLICY "Enable delete for authenticated users only"
ON customer_notices
FOR DELETE
TO authenticated
USING (true);

-- 5. 스키마 캐시 새로고침 (중요!)
NOTIFY pgrst, 'reload schema';

-- 6. 데이터 확인
SELECT COUNT(*) as total FROM customer_notices;

SELECT 
  id,
  title,
  category,
  importance,
  is_pinned
FROM customer_notices
ORDER BY is_pinned DESC, published_at DESC
LIMIT 5;
