-- CRITICAL FIX: DB Customers 데이터 로드 문제 해결
-- 이 스크립트는 반드시 Supabase SQL Editor에서 실행해야 합니다

-- ========================================
-- 1단계: 현재 상태 진단
-- ========================================

-- 테이블 존재 확인
DO $$
BEGIN
    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'db_customers') THEN
        RAISE NOTICE '✅ db_customers 테이블이 존재합니다';
    ELSE
        RAISE NOTICE '❌ db_customers 테이블이 존재하지 않습니다';
    END IF;
END $$;

-- RLS 상태 확인
SELECT 
    'RLS 상태: ' || CASE WHEN rowsecurity THEN '활성화됨 ✅' ELSE '비활성화됨 ❌' END AS rls_status
FROM pg_tables 
WHERE schemaname = 'public' AND tablename = 'db_customers';

-- 현재 데이터 개수
SELECT 'db_customers 테이블에 ' || COUNT(*)::text || '개의 레코드가 있습니다' AS data_count
FROM db_customers;

-- RLS 정책 목록
SELECT 
    '정책: ' || policyname AS policy_info,
    'Command: ' || cmd AS command_type,
    'Roles: ' || array_to_string(roles, ', ') AS applicable_roles
FROM pg_policies
WHERE schemaname = 'public' AND tablename = 'db_customers';

-- ========================================
-- 2단계: 기존 데이터 확인 (있다면)
-- ========================================

SELECT 
    'Sample Data' AS info,
    id, contact_id, customer_name, status, "group", 
    assigned_to, created_at
FROM db_customers 
ORDER BY created_at DESC 
LIMIT 5;

-- ========================================
-- 3단계: 임시로 RLS 비활성화 (디버깅용)
-- ========================================

-- 주의: 이것은 임시 조치입니다. 개발 환경에서만 사용하세요!
ALTER TABLE db_customers DISABLE ROW LEVEL SECURITY;

SELECT '⚠️  RLS가 임시로 비활성화되었습니다 (디버깅용)' AS warning;

-- ========================================
-- 4단계: 기존 테스트 데이터 삭제
-- ========================================

DELETE FROM db_customers WHERE contact_id BETWEEN 1001 AND 1020;

SELECT '🗑️  기존 테스트 데이터가 삭제되었습니다' AS cleanup;

-- ========================================
-- 5단계: 새로운 테스트 데이터 삽입 (올바른 enum 값 사용)
-- ========================================

INSERT INTO db_customers (
    contact_id, customer_name, contact_number, "group",
    assigned_to, assigner, assigned_date, last_contact_date,
    status, is_starred, is_archived, created_at, updated_at
) VALUES
-- Completed 상태 (3개)
(1001, '김민수', '010-1234-5678', 'VIP', 
 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '30 days', NOW() - INTERVAL '2 days',
 'Completed', true, false, NOW(), NOW()),

(1002, '정민호', '010-5678-9012', 'Standard',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '15 days', NOW() - INTERVAL '7 days',
 'Completed', false, false, NOW(), NOW()),

(1003, '임예은', '010-0123-4567', 'Premium',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '50 days', NOW() - INTERVAL '3 days',
 'Completed', false, false, NOW(), NOW()),

-- InProgress 상태 (4개)
(1004, '이지은', '010-2345-6789', 'VIP',
 'sales@nexa.test', 'manager@nexa.test',
 NOW() - INTERVAL '45 days', NOW() - INTERVAL '1 day',
 'InProgress', true, false, NOW(), NOW()),

(1005, '박지훈', '010-3456-7890', 'VIP',
 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '60 days', NOW() - INTERVAL '5 days',
 'InProgress', true, false, NOW(), NOW()),

(1006, '최수진', '010-4567-8901', 'Standard',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '20 days', NOW() - INTERVAL '3 days',
 'InProgress', false, false, NOW(), NOW()),

(1007, '윤재원', '010-7890-1234', 'Premium',
 'sales@nexa.test', 'manager@nexa.test',
 NOW() - INTERVAL '25 days', NOW() - INTERVAL '4 days',
 'InProgress', true, false, NOW(), NOW()),

-- New 상태 (1개)
(1008, '강서연', '010-6789-0123', 'Premium',
 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '10 days', NOW() - INTERVAL '1 day',
 'New', true, false, NOW(), NOW()),

-- NoAnswer 상태 (1개)
(1009, '한지민', '010-8901-2345', 'Standard',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '35 days', NOW() - INTERVAL '8 days',
 'NoAnswer', false, false, NOW(), NOW()),

-- OnHold 상태 (1개)
(1010, '서동현', '010-9012-3456', 'VIP',
 'manager@nexa.test', 'manager@nexa.test',
 NOW() - INTERVAL '40 days', NOW() - INTERVAL '10 days',
 'OnHold', true, false, NOW(), NOW());

SELECT '✅ 10개의 새로운 테스트 데이터가 삽입되었습니다' AS insert_result;

-- ========================================
-- 6단계: 삽입 확인
-- ========================================

SELECT 
    '삽입된 데이터: ' || COUNT(*)::text || '개' AS verification
FROM db_customers 
WHERE contact_id BETWEEN 1001 AND 1010;

-- Status 분포 확인
SELECT 
    status,
    COUNT(*) AS count,
    '(' || ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM db_customers WHERE contact_id BETWEEN 1001 AND 1010), 1)::text || '%)' AS percentage
FROM db_customers 
WHERE contact_id BETWEEN 1001 AND 1010
GROUP BY status 
ORDER BY count DESC;

-- ========================================
-- 7단계: 전체 데이터 확인
-- ========================================

SELECT 
    id, 
    contact_id, 
    customer_name, 
    contact_number,
    "group", 
    status, 
    assigned_to,
    assigner,
    is_starred, 
    is_archived,
    TO_CHAR(assigned_date, 'YYYY-MM-DD') AS assigned_date,
    TO_CHAR(last_contact_date, 'YYYY-MM-DD') AS last_contact_date,
    TO_CHAR(created_at, 'YYYY-MM-DD HH24:MI:SS') AS created_at
FROM db_customers 
WHERE contact_id BETWEEN 1001 AND 1010
ORDER BY contact_id;

-- ========================================
-- 8단계: RLS 재활성화 및 정책 재생성
-- ========================================

-- RLS 재활성화
ALTER TABLE db_customers ENABLE ROW LEVEL SECURITY;

SELECT '✅ RLS가 다시 활성화되었습니다' AS rls_status;

-- 기존 정책 삭제
DROP POLICY IF EXISTS "Allow authenticated users to read db_customers" ON db_customers;
DROP POLICY IF EXISTS "Allow authenticated users to insert db_customers" ON db_customers;
DROP POLICY IF EXISTS "Allow authenticated users to update db_customers" ON db_customers;
DROP POLICY IF EXISTS "Allow authenticated users to delete db_customers" ON db_customers;

SELECT '🗑️  기존 RLS 정책이 삭제되었습니다' AS policy_cleanup;

-- 새 정책 생성 (더 명확하고 관대한 정책)
CREATE POLICY "Allow authenticated users to read db_customers"
    ON db_customers
    FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Allow authenticated users to insert db_customers"
    ON db_customers
    FOR INSERT
    TO authenticated
    WITH CHECK (true);

CREATE POLICY "Allow authenticated users to update db_customers"
    ON db_customers
    FOR UPDATE
    TO authenticated
    USING (true)
    WITH CHECK (true);

CREATE POLICY "Allow authenticated users to delete db_customers"
    ON db_customers
    FOR DELETE
    TO authenticated
    USING (true);

SELECT '✅ 새로운 RLS 정책이 생성되었습니다' AS policy_created;

-- ========================================
-- 9단계: 최종 확인
-- ========================================

-- RLS 정책 목록 확인 (간단한 버전)
SELECT 
    policyname AS policy_name,
    cmd AS command,
    array_to_string(roles, ', ') AS roles
FROM pg_policies
WHERE schemaname = 'public' AND tablename = 'db_customers'
ORDER BY cmd, policyname;

SELECT '✅ RLS 정책 목록이 표시되었습니다' AS policy_list_status;

-- 최종 데이터 개수
SELECT 
    '✅ 최종 확인: db_customers 테이블에 ' || COUNT(*)::text || '개의 레코드가 있습니다' AS final_count
FROM db_customers;

-- ========================================
-- 완료 메시지
-- ========================================

SELECT '
===========================================
✅ 진단 및 수정 완료!
===========================================

다음 단계:
1. 애플리케이션을 재시작하세요
2. 브라우저에서 /db/customer/all 페이지로 이동하세요
3. 브라우저 콘솔(F12)에서 로그를 확인하세요
4. 10개의 테스트 데이터가 표시되어야 합니다

문제가 지속되면:
- 서버 로그에서 "Loaded X DB customer records" 메시지 확인
- Supabase 연결 설정 확인 (.env 파일)
- 브라우저 네트워크 탭에서 API 요청 확인

===========================================
' AS completion_message;
