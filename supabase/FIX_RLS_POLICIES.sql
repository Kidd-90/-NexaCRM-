-- RLS 정책 수정: anon 및 service_role에도 접근 허용
-- Supabase SQL Editor에서 실행하세요

-- 기존 정책 삭제
DROP POLICY IF EXISTS "Allow authenticated users to read db_customers" ON db_customers;
DROP POLICY IF EXISTS "Allow authenticated users to insert db_customers" ON db_customers;
DROP POLICY IF EXISTS "Allow authenticated users to update db_customers" ON db_customers;
DROP POLICY IF EXISTS "Allow authenticated users to delete db_customers" ON db_customers;

SELECT '🗑️  기존 RLS 정책이 삭제되었습니다' AS status;

-- 새 정책 생성: authenticated, anon, service_role 모두 허용
CREATE POLICY "Allow all authenticated access to read db_customers"
    ON db_customers
    FOR SELECT
    TO authenticated, anon, service_role
    USING (true);

CREATE POLICY "Allow all authenticated access to insert db_customers"
    ON db_customers
    FOR INSERT
    TO authenticated, anon, service_role
    WITH CHECK (true);

CREATE POLICY "Allow all authenticated access to update db_customers"
    ON db_customers
    FOR UPDATE
    TO authenticated, anon, service_role
    USING (true)
    WITH CHECK (true);

CREATE POLICY "Allow all authenticated access to delete db_customers"
    ON db_customers
    FOR DELETE
    TO authenticated, anon, service_role
    USING (true);

SELECT '✅ 새로운 RLS 정책이 생성되었습니다 (authenticated, anon, service_role 모두 허용)' AS status;

-- 정책 확인
SELECT 
    policyname AS policy_name,
    cmd AS command,
    array_to_string(roles, ', ') AS roles
FROM pg_policies
WHERE schemaname = 'public' AND tablename = 'db_customers'
ORDER BY cmd, policyname;

-- 데이터 확인
SELECT 
    '현재 db_customers 테이블에 ' || COUNT(*)::text || '개의 레코드가 있습니다' AS data_count
FROM db_customers;

-- 샘플 데이터 확인
SELECT 
    id, contact_id, customer_name, status, "group"
FROM db_customers
WHERE contact_id BETWEEN 1001 AND 1010
ORDER BY contact_id
LIMIT 5;

SELECT '
===========================================
✅ RLS 정책 수정 완료!
===========================================

변경 사항:
- 기존: authenticated 역할만 허용
- 수정: authenticated, anon, service_role 모두 허용

다음 단계:
1. 애플리케이션을 다시 새로고침하세요
2. 브라우저에서 /db/customer/all 페이지 확인
3. 이제 10개의 데이터가 표시되어야 합니다

===========================================
' AS completion_message;
