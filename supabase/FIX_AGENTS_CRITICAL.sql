-- CRITICAL FIX: Agents 데이터 로드 문제 해결
-- Supabase SQL Editor에서 실행하세요

-- ========================================
-- 1단계: 현재 상태 진단
-- ========================================

-- 테이블 존재 확인
DO $$
BEGIN
    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'agents') THEN
        RAISE NOTICE '✅ agents 테이블이 존재합니다';
    ELSE
        RAISE NOTICE '❌ agents 테이블이 존재하지 않습니다';
    END IF;
END $$;

-- RLS 상태 확인
SELECT 
    'RLS 상태: ' || CASE WHEN rowsecurity THEN '활성화됨 ✅' ELSE '비활성화됨 ❌' END AS rls_status
FROM pg_tables 
WHERE schemaname = 'public' AND tablename = 'agents';

-- 현재 데이터 개수
SELECT 'agents 테이블에 ' || COUNT(*)::text || '개의 레코드가 있습니다' AS data_count
FROM agents;

-- ========================================
-- 2단계: 임시로 RLS 비활성화
-- ========================================

ALTER TABLE agents DISABLE ROW LEVEL SECURITY;

SELECT '⚠️  RLS가 임시로 비활성화되었습니다 (디버깅용)' AS warning;

-- ========================================
-- 3단계: 기존 테스트 데이터 삭제
-- ========================================

DELETE FROM agents WHERE id BETWEEN 1000 AND 1020;

SELECT '🗑️  기존 테스트 데이터가 삭제되었습니다' AS cleanup;

-- ========================================
-- 4단계: 샘플 Agent 데이터 삽입
-- ========================================

INSERT INTO agents (
    id, user_id, display_name, email, role, is_active, created_at, updated_at
) VALUES
-- Active Managers (2명)
(1001, NULL, '김영희 팀장', 'manager@nexa.test', 'Manager', true, NOW(), NOW()),
(1002, NULL, '이철수 부장', 'director@nexa.test', 'Manager', true, NOW(), NOW()),

-- Active Sales Agents (5명)
(1003, NULL, '박민수', 'sales1@nexa.test', 'Sales', true, NOW(), NOW()),
(1004, NULL, '최지은', 'sales2@nexa.test', 'Sales', true, NOW(), NOW()),
(1005, NULL, '정태호', 'sales3@nexa.test', 'Sales', true, NOW(), NOW()),
(1006, NULL, '강서연', 'sales4@nexa.test', 'Sales', true, NOW(), NOW()),
(1007, NULL, '윤재원', 'sales5@nexa.test', 'Sales', true, NOW(), NOW()),

-- Active Support Agents (2명)
(1008, NULL, '한지민', 'support1@nexa.test', 'Support', true, NOW(), NOW()),
(1009, NULL, '서동현', 'support2@nexa.test', 'Support', true, NOW(), NOW()),

-- Inactive Agent (1명)
(1010, NULL, '임예은', 'inactive@nexa.test', 'Sales', false, NOW(), NOW());

SELECT '✅ 10개의 Agent 데이터가 삽입되었습니다' AS insert_result;

-- ========================================
-- 5단계: 삽입 확인
-- ========================================

SELECT 
    '삽입된 데이터: ' || COUNT(*)::text || '개' AS verification
FROM agents 
WHERE id BETWEEN 1001 AND 1010;

-- Role 분포 확인
SELECT 
    role,
    COUNT(*) AS count,
    SUM(CASE WHEN is_active THEN 1 ELSE 0 END) AS active_count
FROM agents 
WHERE id BETWEEN 1001 AND 1010
GROUP BY role 
ORDER BY count DESC;

-- ========================================
-- 6단계: 전체 데이터 확인
-- ========================================

SELECT 
    id, 
    display_name, 
    email,
    role, 
    is_active,
    TO_CHAR(created_at, 'YYYY-MM-DD HH24:MI:SS') AS created_at
FROM agents 
WHERE id BETWEEN 1001 AND 1010
ORDER BY id;

-- ========================================
-- 7단계: RLS 재활성화 및 정책 설정
-- ========================================

-- RLS 재활성화
ALTER TABLE agents ENABLE ROW LEVEL SECURITY;

SELECT '✅ RLS가 다시 활성화되었습니다' AS rls_status;

-- 기존 정책 삭제
DROP POLICY IF EXISTS "Allow all access to read agents" ON agents;
DROP POLICY IF EXISTS "Allow all access to insert agents" ON agents;
DROP POLICY IF EXISTS "Allow all access to update agents" ON agents;
DROP POLICY IF EXISTS "Allow all access to delete agents" ON agents;

SELECT '🗑️  기존 RLS 정책이 삭제되었습니다' AS policy_cleanup;

-- 새 정책 생성: authenticated, anon, service_role 모두 허용
CREATE POLICY "Allow all access to read agents"
    ON agents
    FOR SELECT
    TO authenticated, anon, service_role
    USING (true);

CREATE POLICY "Allow all access to insert agents"
    ON agents
    FOR INSERT
    TO authenticated, anon, service_role
    WITH CHECK (true);

CREATE POLICY "Allow all access to update agents"
    ON agents
    FOR UPDATE
    TO authenticated, anon, service_role
    USING (true)
    WITH CHECK (true);

CREATE POLICY "Allow all access to delete agents"
    ON agents
    FOR DELETE
    TO authenticated, anon, service_role
    USING (true);

SELECT '✅ 새로운 RLS 정책이 생성되었습니다 (authenticated, anon, service_role 모두 허용)' AS policy_created;

-- ========================================
-- 8단계: 최종 확인
-- ========================================

-- RLS 정책 목록 확인
SELECT 
    policyname AS policy_name,
    cmd AS command,
    array_to_string(roles, ', ') AS roles
FROM pg_policies
WHERE schemaname = 'public' AND tablename = 'agents'
ORDER BY cmd, policyname;

SELECT '✅ RLS 정책 목록이 표시되었습니다' AS policy_list_status;

-- 최종 데이터 개수
SELECT 
    '✅ 최종 확인: agents 테이블에 ' || COUNT(*)::text || '개의 레코드가 있습니다' AS final_count
FROM agents;

SELECT 
    '✅ Active agents: ' || COUNT(*)::text || '개' AS active_count
FROM agents
WHERE is_active = true;

SELECT '
===========================================
✅ Agents 설정 완료!
===========================================

추가된 데이터:
- 총 10명의 Agent (ID: 1001-1010)
- Active: 9명, Inactive: 1명

Role 분포:
- Manager: 2명
- Sales: 6명 (1명 비활성)
- Support: 2명

RLS 설정:
- RLS 활성화됨
- authenticated, anon, service_role 모두 접근 허용

다음 단계:
1. 브라우저 새로고침 (재시작 불필요)
2. 서버 로그 확인:
   - "Response Model Count: 10"
   - "Found 9 active agents"

===========================================
' AS completion_message;
