-- Diagnostic and Fix Script for db_customers table
-- Run this in Supabase SQL Editor

-- Step 1: Check if table exists and has RLS enabled
SELECT
    schemaname,
    tablename,
    rowsecurity
FROM pg_tables
WHERE tablename = 'db_customers';

-- Step 2: Check existing policies
SELECT
    schemaname,
    tablename,
    policyname,
    permissive,
    roles,
    cmd,
    qual,
    with_check
FROM pg_policies
WHERE tablename = 'db_customers';

-- Step 3: Check current data count
SELECT COUNT(*) as total_records FROM db_customers;

-- Step 4: Check data with details
SELECT 
    id,
    contact_id,
    customer_name,
    status,
    is_archived,
    created_at
FROM db_customers
ORDER BY created_at DESC
LIMIT 10;

-- Step 5: If no data exists, insert test data with correct enum values
-- Delete any existing test data first
DELETE FROM db_customers WHERE contact_id BETWEEN 1001 AND 1020;

-- Insert fresh test data
INSERT INTO db_customers (
  contact_id,
  customer_name,
  contact_number,
  "group",
  assigned_to,
  assigner,
  assigned_date,
  last_contact_date,
  status,
  is_starred,
  is_archived,
  gender,
  address,
  region,
  notes,
  tags
) VALUES
(1001, '김민수', '010-1234-5678', 'VIP', 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '30 days', NOW() - INTERVAL '2 days', 'Completed', true, false,
 'Male', '서울시 강남구 테헤란로 123', 'Seoul',
 '장기 고객, 매우 만족도 높음. 프리미엄 서비스 이용 중.', 'VIP,장기고객,프리미엄'),
 
(1002, '이지은', '010-2345-6789', 'VIP', 'sales@nexa.test', 'manager@nexa.test',
 NOW() - INTERVAL '45 days', NOW() - INTERVAL '1 day', 'InProgress', true, false,
 'Female', '서울시 서초구 서초대로 456', 'Seoul',
 '기업 대표, 추가 서비스 확장 논의 중.', 'VIP,기업,확장가능'),
 
(1003, '박지훈', '010-3456-7890', 'VIP', 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '60 days', NOW() - INTERVAL '5 days', 'InProgress', true, false,
 'Male', '경기도 성남시 분당구 판교로 789', 'Gyeonggi',
 '기술 관련 문의 많음. 전문적인 대응 필요.', 'VIP,IT,기업'),
 
(1004, '최수진', '010-4567-8901', 'Standard', 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '20 days', NOW() - INTERVAL '3 days', 'InProgress', false, false,
 'Female', '서울시 마포구 월드컵북로 321', 'Seoul',
 '마케팅 분야 종사, 추가 서비스 관심 보임.', 'Standard,마케팅'),
 
(1005, '정민호', '010-5678-9012', 'Standard', 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '15 days', NOW() - INTERVAL '7 days', 'Completed', false, false,
 'Male', '부산시 해운대구 해운대로 654', 'Busan',
 '기술 지원 요청 빈번. 개발팀과 협력 중.', 'Standard,스타트업,기술지원'),
 
(1006, '강서영', '010-6789-0123', 'Standard', 'sales@nexa.test', 'manager@nexa.test',
 NOW() - INTERVAL '10 days', NOW() - INTERVAL '4 days', 'New', false, false,
 'Female', '인천시 연수구 송도과학로 987', 'Incheon',
 '교육 분야 사업. 초기 단계 지원 필요.', 'Standard,교육,신규'),
 
(1007, '윤태희', '010-7890-1234', 'Prospect', 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days', 'New', false, false,
 'Male', '대전시 유성구 대학로 147', 'Daejeon',
 '서비스 도입 검토 중. 예산 협의 필요.', 'Prospect,검토중'),
 
(1008, '한지민', '010-8901-2345', 'Prospect', 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', 'New', false, false,
 'Female', '광주시 북구 첨단과학로 258', 'Gwangju',
 '제품 시연 후 긍정적 반응. 추가 미팅 예정.', 'Prospect,데모완료'),
 
(1009, '오준석', '010-9012-3456', 'Standard', 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '90 days', NOW() - INTERVAL '60 days', 'NoAnswer', false, false,
 'Male', '울산시 남구 삼산로 369', 'Ulsan',
 '마지막 접촉 후 2개월 경과. 재연락 시도 필요.', 'Inactive,재활성화대상'),
 
(1010, '송하은', '010-0123-4567', 'Standard', 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '120 days', NOW() - INTERVAL '90 days', 'OnHold', false, false,
 'Female', '대구시 수성구 동대구로 741', 'Daegu',
 '가격 문제로 서비스 중단. 프로모션 제안 검토.', 'Inactive,가격민감');

-- Step 6: Verify insert
SELECT COUNT(*) as inserted_count FROM db_customers WHERE contact_id BETWEEN 1001 AND 1020;

-- Step 7: Test query as authenticated user would see it
SELECT 
    contact_id,
    customer_name,
    contact_number,
    "group",
    assigned_to,
    status,
    is_starred,
    is_archived
FROM db_customers
WHERE is_archived = false
ORDER BY assigned_date DESC;
