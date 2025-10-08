# 🎯 Admin 계정 생성 - 완전 가이드

## 📋 사전 준비 (필수!)

### Supabase Dashboard에서 Auth 사용자 먼저 생성

1. **Supabase Dashboard** 접속: https://supabase.com/dashboard
2. 프로젝트 선택
3. 왼쪽 메뉴에서 **Authentication** → **Users** 클릭
4. **Add User** 버튼 클릭
5. 다음 정보 입력:
   ```
   Email: admin@nexa.test
   Password: admin123
   ☑️ Auto Confirm User (반드시 체크!)
   ☐ Send Magic Link (체크 해제)
   ```
6. **Create User** 버튼 클릭
7. 생성 완료 - Users 목록에서 확인

---

## ⚡ SQL 실행

### Supabase Dashboard SQL Editor에서 실행

1. 왼쪽 메뉴에서 **SQL Editor** 클릭
2. **New Query** 버튼 클릭
3. `/scripts/create-admin-complete.sql` 파일 내용을 **전체 복사**
4. SQL Editor에 붙여넣기
5. **Run** (또는 Cmd/Ctrl + Enter) 버튼 클릭

### 예상 출력

```
📋 Found auth user: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
✅ Created app_users record with CUID: admin_xxxxxxxxxxxxxxxx
✅ Created/Updated user_infos record
✅ Created/Updated profiles record
✅ Created/Updated organization_users record
✅ Added Admin role to user_roles table

🎉 Admin account successfully created and linked!

📝 Login credentials:
   Email/Username: admin@nexa.test or admin
   Password: admin123

🔑 Auth User ID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
🆔 CUID: admin_xxxxxxxxxxxxxxxx
```

### 확인 테이블

마지막 두 개의 SELECT 쿼리 결과가 표시됩니다:

#### FINAL VERIFICATION
| email | auth_user_id | username | full_name | user_role | org_role | assigned_roles |
|-------|--------------|----------|-----------|-----------|----------|----------------|
| admin@nexa.test | uuid | admin | System Administrator | Admin | Admin | {Admin} |

#### VIEW VERIFICATION  
| cuid | auth_user_id | email | username | role_codes |
|------|--------------|-------|----------|------------|
| admin_xxx | uuid | admin@nexa.test | admin | {Admin} |

모든 값이 채워져 있으면 **성공**입니다! ✅

---

## 🔐 로그인 테스트

### 1. 서버 실행
```bash
cd src/NexaCRM.WebServer
dotnet run
```

### 2. 브라우저에서 로그인
```
URL: http://localhost:5000/login

로그인 정보:
- Username: admin (또는 admin@nexa.test)
- Password: admin123
```

---

## 🔧 문제 해결

### "Auth user not found" 에러
→ Authentication > Users에서 먼저 사용자를 생성해야 합니다.

### "Admin account already exists" 로그
→ 이미 계정이 있습니다. 기존 계정을 삭제하려면:

```sql
-- 기존 admin 계정 완전 삭제
DELETE FROM user_roles WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
DELETE FROM organization_users WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
DELETE FROM profiles WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
DELETE FROM user_infos WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
DELETE FROM app_users WHERE email = 'admin@nexa.test';

-- Authentication > Users에서도 수동 삭제 필요
```

### 로그인 시 "관리자 승인 대기" 에러
→ 계정 상태가 'Pending'입니다. SQL로 수정:

```sql
UPDATE app_users 
SET status = 'active' 
WHERE email = 'admin@nexa.test';

UPDATE user_infos 
SET status = 'Active' 
WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
```

### 현재 admin 계정 상태 확인

```sql
SELECT 
    au.email,
    au.auth_user_id,
    au.status AS app_status,
    ui.username,
    ui.status AS user_status,
    ou.role AS org_role,
    ur.role_code AS user_role
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
LEFT JOIN organization_users ou ON ou.user_cuid = au.cuid
LEFT JOIN user_roles ur ON ur.user_cuid = au.cuid
WHERE au.email = 'admin@nexa.test';
```

---

## ✅ 체크리스트

- [ ] Supabase Authentication > Users에서 admin@nexa.test 사용자 생성
- [ ] Auto Confirm User 체크 확인
- [ ] SQL Editor에서 create-admin-complete.sql 실행
- [ ] 모든 ✅ 메시지 확인
- [ ] FINAL VERIFICATION 테이블에서 모든 값 확인
- [ ] 서버 실행
- [ ] 로그인 테스트 성공

완료! 🎉
