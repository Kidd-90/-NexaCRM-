# DB Customers 데이터 로드 문제 해결 가이드

## 문제 현황
- 애플리케이션에서 `GetAllDbListAsync()` 호출 시 0개의 레코드 반환
- 사용자 인증은 성공 (3c084ae0-4374-43f7-8f98-fbb1d81c95d3)

## 해결 단계

### 1단계: Supabase에서 데이터 확인

Supabase SQL Editor에 접속하여 다음 스크립트를 실행하세요:
`/supabase/diagnose_and_fix_db_customers.sql`

이 스크립트는:
1. 테이블 존재 여부 확인
2. RLS 정책 확인
3. 데이터 개수 확인
4. 테스트 데이터 삽입
5. 데이터 검증

### 2단계: RLS 정책 확인

다음 쿼리로 RLS가 올바르게 설정되었는지 확인:

```sql
SELECT 
    schemaname,
    tablename,
    policyname,
    roles,
    cmd
FROM pg_policies
WHERE tablename = 'db_customers';
```

예상 결과:
- `Allow authenticated users to read db_customers` (SELECT)
- 다른 CRUD 정책들

### 3단계: 수동으로 테스트 데이터 삽입

```sql
-- 기존 테스트 데이터 삭제
DELETE FROM db_customers WHERE contact_id BETWEEN 1001 AND 1010;

-- 새 테스트 데이터 삽입 (status는 반드시 DbStatus enum 값 사용)
INSERT INTO db_customers (
  contact_id, customer_name, contact_number, "group",
  assigned_to, assigner, assigned_date, last_contact_date,
  status, is_starred, is_archived
) VALUES
(1001, '김민수', '010-1234-5678', 'VIP', 
 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '30 days', NOW() - INTERVAL '2 days',
 'Completed', true, false),
 
(1002, '이지은', '010-2345-6789', 'VIP',
 'sales@nexa.test', 'manager@nexa.test',
 NOW() - INTERVAL '45 days', NOW() - INTERVAL '1 day',
 'InProgress', true, false),
 
(1003, '박지훈', '010-3456-7890', 'VIP',
 'manager@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '60 days', NOW() - INTERVAL '5 days',
 'InProgress', true, false),
 
(1004, '최수진', '010-4567-8901', 'Standard',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '20 days', NOW() - INTERVAL '3 days',
 'InProgress', false, false),
 
(1005, '정민호', '010-5678-9012', 'Standard',
 'sales@nexa.test', 'sales@nexa.test',
 NOW() - INTERVAL '15 days', NOW() - INTERVAL '7 days',
 'Completed', false, false);

-- 데이터 확인
SELECT COUNT(*) FROM db_customers;
SELECT * FROM db_customers ORDER BY created_at DESC LIMIT 5;
```

### 4단계: 애플리케이션 로그 확인

애플리케이션을 다시 시작하고 브라우저 콘솔에서 다음 로그를 확인:

```
[AllDbListPage] Loaded X customers from database
[AllDbListPage] Found X groups
[AllDbListPage] After filtering: X customers
```

서버 로그에서:
```
Loading DB customers from Supabase...
Executing query on db_customers table...
Loaded X DB customer records from Supabase
Mapped X DB customers successfully
```

### 5단계: RLS 문제인 경우 임시 해결책

만약 RLS가 문제라면 임시로 다음을 실행:

```sql
-- RLS 임시 비활성화 (개발 환경에서만!)
ALTER TABLE db_customers DISABLE ROW LEVEL SECURITY;

-- 테스트 후 다시 활성화
ALTER TABLE db_customers ENABLE ROW LEVEL SECURITY;
```

### 6단계: 정책 재생성

문제가 지속되면 정책을 재생성:

```sql
-- 기존 정책 삭제
DROP POLICY IF EXISTS "Allow authenticated users to read db_customers" ON db_customers;
DROP POLICY IF EXISTS "Allow authenticated users to insert db_customers" ON db_customers;
DROP POLICY IF EXISTS "Allow authenticated users to update db_customers" ON db_customers;
DROP POLICY IF EXISTS "Allow authenticated users to delete db_customers" ON db_customers;

-- 정책 재생성
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
```

## 체크리스트

- [ ] Supabase에 데이터가 실제로 존재하는가?
- [ ] RLS가 활성화되어 있는가?
- [ ] RLS 정책이 올바르게 설정되어 있는가?
- [ ] status 컬럼의 값이 DbStatus enum과 일치하는가? (New, InProgress, NoAnswer, Completed, OnHold)
- [ ] 사용자가 인증되었는가?
- [ ] Supabase 연결 설정이 올바른가?

## 일반적인 문제

1. **Status 값 불일치**: 'Active', 'Inactive' 등의 값 대신 반드시 DbStatus enum 값 사용
2. **RLS 정책**: authenticated role에 대한 정책이 없거나 조건이 너무 제한적
3. **빈 데이터베이스**: 마이그레이션이 실행되지 않음
4. **연결 문제**: Supabase URL이나 API Key가 잘못됨

## 참고 파일

- Schema: `/supabase/migrations/schema.sql`
- RLS Policies: `/supabase/migrations/20251007000001_db_customers_rls_policies.sql`
- Sample Data: `/supabase/migrations/20251007000002_insert_db_customers_sample_data.sql`
- Diagnostic Script: `/supabase/diagnose_and_fix_db_customers.sql`
