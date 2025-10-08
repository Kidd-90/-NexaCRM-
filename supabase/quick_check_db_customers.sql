-- 빠른 DB Customers 진단 및 수정
-- Supabase SQL Editor에서 실행하세요

-- 1. 테이블 존재 확인
SELECT EXISTS (
  SELECT FROM pg_tables
  WHERE schemaname = 'public' AND tablename = 'db_customers'
) AS table_exists;

-- 2. 현재 데이터 개수
SELECT COUNT(*) AS current_count FROM db_customers;

-- 3. 현재 데이터 샘플 (있다면)
SELECT 
  id, contact_id, customer_name, status, "group", 
  assigned_to, created_at
FROM db_customers 
ORDER BY created_at DESC 
LIMIT 5;

-- 4. RLS 상태 확인
SELECT 
  schemaname, tablename, 
  rowsecurity AS rls_enabled
FROM pg_tables 
WHERE tablename = 'db_customers';

-- 5. 기존 테스트 데이터 삭제
DELETE FROM db_customers WHERE contact_id BETWEEN 1001 AND 1015;

-- 6. 올바른 테스트 데이터 삽입 (DbStatus enum 값 사용)
INSERT INTO db_customers (
  contact_id, customer_name, contact_number, "group",
  assigned_to, assigner, assigned_date, last_contact_date,
  status, is_starred, is_archived, created_at, updated_at
) VALUES
-- VIP 그룹 - Completed 상태
(1001, '김민수', '010-1234-5678', 'VIP', 
 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '30 days', NOW() - INTERVAL '2 days',
 'Completed', true, false, NOW(), NOW()),

-- VIP 그룹 - InProgress 상태
(1002, '이지은', '010-2345-6789', 'VIP',
 'sales@nexa.test', 'manager@nexa.test',
 NOW() - INTERVAL '45 days', NOW() - INTERVAL '1 day',
 'InProgress', true, false, NOW(), NOW()),

(1003, '박지훈', '010-3456-7890', 'VIP',
 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '60 days', NOW() - INTERVAL '5 days',
 'InProgress', true, false, NOW(), NOW()),

-- Standard 그룹 - InProgress 상태
(1004, '최수진', '010-4567-8901', 'Standard',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '20 days', NOW() - INTERVAL '3 days',
 'InProgress', false, false, NOW(), NOW()),

(1005, '정민호', '010-5678-9012', 'Standard',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '15 days', NOW() - INTERVAL '7 days',
 'Completed', false, false, NOW(), NOW()),

-- Premium 그룹 - New 상태
(1006, '강서연', '010-6789-0123', 'Premium',
 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '10 days', NOW() - INTERVAL '1 day',
 'New', true, false, NOW(), NOW()),

(1007, '윤재원', '010-7890-1234', 'Premium',
 'sales@nexa.test', 'manager@nexa.test',
 NOW() - INTERVAL '25 days', NOW() - INTERVAL '4 days',
 'InProgress', true, false, NOW(), NOW()),

-- Standard 그룹 - NoAnswer 상태
(1008, '한지민', '010-8901-2345', 'Standard',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '35 days', NOW() - INTERVAL '8 days',
 'NoAnswer', false, false, NOW(), NOW()),

-- VIP 그룹 - OnHold 상태
(1009, '서동현', '010-9012-3456', 'VIP',
 'manager@nexa.test', 'manager@nexa.test',
 NOW() - INTERVAL '40 days', NOW() - INTERVAL '10 days',
 'OnHold', true, false, NOW(), NOW()),

-- Premium 그룹 - Completed 상태
(1010, '임예은', '010-0123-4567', 'Premium',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '50 days', NOW() - INTERVAL '3 days',
 'Completed', false, false, NOW(), NOW());

-- 7. 삽입 확인
SELECT COUNT(*) AS inserted_count 
FROM db_customers 
WHERE contact_id BETWEEN 1001 AND 1010;

-- 8. 모든 status 값 확인 (enum과 일치하는지)
SELECT DISTINCT status, COUNT(*) 
FROM db_customers 
GROUP BY status 
ORDER BY status;

-- 9. 최종 데이터 확인
SELECT 
  id, contact_id, customer_name, contact_number,
  "group", status, assigned_to, 
  is_starred, is_archived,
  TO_CHAR(created_at, 'YYYY-MM-DD HH24:MI:SS') AS created_at
FROM db_customers 
WHERE contact_id BETWEEN 1001 AND 1010
ORDER BY contact_id;

-- 성공 메시지
SELECT '✅ 테스트 데이터가 성공적으로 삽입되었습니다!' AS result;
