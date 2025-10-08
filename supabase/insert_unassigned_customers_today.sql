-- 오늘 날짜로 분배 안된 고객 리스트 추가
-- Supabase SQL Editor에서 실행하세요

-- 기존 테스트 데이터(contact_id 2001-2020) 삭제
DELETE FROM db_customers WHERE contact_id BETWEEN 2001 AND 2020;

SELECT '🗑️  기존 미배정 테스트 데이터가 삭제되었습니다' AS status;

-- 오늘 날짜로 분배 안된(assigned_to가 NULL 또는 빈 문자열) 고객 데이터 삽입
INSERT INTO db_customers (
    contact_id, customer_name, contact_number, "group",
    assigned_to, assigner, assigned_date, last_contact_date,
    status, is_starred, is_archived, created_at, updated_at
) VALUES
-- 미배정 고객 - New 상태 (5명)
(2001, '이서준', '010-1111-2222', 'Standard',
 NULL, NULL, NOW(), NOW(),
 'New', false, false, NOW(), NOW()),

(2002, '박하윤', '010-2222-3333', 'Standard',
 '', NULL, NOW(), NOW(),
 'New', false, false, NOW(), NOW()),

(2003, '최민재', '010-3333-4444', 'VIP',
 NULL, NULL, NOW(), NOW(),
 'New', true, false, NOW(), NOW()),

(2004, '강서아', '010-4444-5555', 'Premium',
 '', NULL, NOW(), NOW(),
 'New', false, false, NOW(), NOW()),

(2005, '윤지호', '010-5555-6666', 'Standard',
 NULL, NULL, NOW(), NOW(),
 'New', false, false, NOW(), NOW()),

-- 미배정 고객 - InProgress 상태 (3명) - 이전에 담당자가 있었지만 현재는 미배정
(2006, '정유나', '010-6666-7777', 'VIP',
 '', 'sales@nexa.test', NOW() - INTERVAL '5 days', NOW() - INTERVAL '1 day',
 'InProgress', true, false, NOW(), NOW()),

(2007, '김도윤', '010-7777-8888', 'Premium',
 NULL, 'manager@nexa.test', NOW() - INTERVAL '3 days', NOW() - INTERVAL '1 day',
 'InProgress', false, false, NOW(), NOW()),

(2008, '이채원', '010-8888-9999', 'Standard',
 '', 'sales@nexa.test', NOW() - INTERVAL '7 days', NOW() - INTERVAL '2 days',
 'InProgress', false, false, NOW(), NOW()),

-- 미배정 고객 - NoAnswer 상태 (2명)
(2009, '박시우', '010-9999-0000', 'Standard',
 NULL, NULL, NOW(), NOW() - INTERVAL '3 days',
 'NoAnswer', false, false, NOW(), NOW()),

(2010, '최서연', '010-0000-1111', 'Premium',
 '', NULL, NOW(), NOW() - INTERVAL '5 days',
 'NoAnswer', false, false, NOW(), NOW());

SELECT '✅ 10개의 미배정 고객 데이터가 추가되었습니다' AS insert_result;

-- 삽입 확인
SELECT 
    '삽입된 미배정 고객: ' || COUNT(*)::text || '개' AS verification
FROM db_customers 
WHERE contact_id BETWEEN 2001 AND 2010
  AND (assigned_to IS NULL OR assigned_to = '');

-- Status 분포 확인
SELECT 
    status,
    COUNT(*) AS count,
    '(' || ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM db_customers WHERE contact_id BETWEEN 2001 AND 2010), 1)::text || '%)' AS percentage
FROM db_customers 
WHERE contact_id BETWEEN 2001 AND 2010
GROUP BY status 
ORDER BY count DESC;

-- 전체 미배정 고객 확인
SELECT 
    id, 
    contact_id, 
    customer_name, 
    contact_number,
    "group", 
    status, 
    COALESCE(assigned_to, '미배정') AS assigned_to,
    COALESCE(assigner, '미배정') AS assigner,
    is_starred, 
    is_archived,
    TO_CHAR(assigned_date, 'YYYY-MM-DD HH24:MI:SS') AS assigned_date,
    TO_CHAR(last_contact_date, 'YYYY-MM-DD HH24:MI:SS') AS last_contact_date,
    TO_CHAR(created_at, 'YYYY-MM-DD HH24:MI:SS') AS created_at
FROM db_customers 
WHERE contact_id BETWEEN 2001 AND 2010
ORDER BY contact_id;

-- 전체 고객 수 (배정 + 미배정)
SELECT 
    '총 고객 수: ' || COUNT(*)::text || '개' AS total_customers,
    '배정된 고객: ' || SUM(CASE WHEN assigned_to IS NOT NULL AND assigned_to != '' THEN 1 ELSE 0 END)::text || '개' AS assigned_customers,
    '미배정 고객: ' || SUM(CASE WHEN assigned_to IS NULL OR assigned_to = '' THEN 1 ELSE 0 END)::text || '개' AS unassigned_customers
FROM db_customers;

SELECT '
===========================================
✅ 미배정 고객 데이터 추가 완료!
===========================================

추가된 데이터:
- 총 10명의 미배정 고객
- Contact ID: 2001-2010
- 오늘 날짜(assigned_date = NOW())
- assigned_to: NULL 또는 빈 문자열

상태 분포:
- New: 5명 (완전 새로운 미배정 고객)
- InProgress: 3명 (이전 담당자가 있었지만 현재 미배정)
- NoAnswer: 2명 (연락 안됨)

그룹 분포:
- Standard: 4명
- VIP: 2명
- Premium: 3명
- 별표(starred): 2명

다음 단계:
1. 브라우저에서 페이지 새로고침(F5)
2. 이제 총 20명의 고객이 표시됩니다
   - 배정된 고객: 10명 (contact_id 1001-1010)
   - 미배정 고객: 10명 (contact_id 2001-2010)

===========================================
' AS completion_message;
