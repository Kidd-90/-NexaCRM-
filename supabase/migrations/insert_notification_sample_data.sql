-- ============================================================================
-- 알림 피드 샘플 데이터 삽입
-- ============================================================================
-- 이 스크립트는 notification_feed 테이블에 다양한 유형의 샘플 알림을 추가합니다.
-- 사용법: Supabase SQL Editor에서 실행하거나 psql로 실행
-- 주의: user_id는 실제 auth.users 테이블의 UUID로 교체해야 합니다.
-- ============================================================================

-- 먼저 테스트용 사용자 ID를 변수로 설정 (실제 환경에서는 실제 user_id로 교체)
-- DO $$
-- DECLARE
--   test_user_id UUID;
-- BEGIN
--   -- 첫 번째 사용자의 ID를 가져옴
--   SELECT id INTO test_user_id FROM auth.users LIMIT 1;
  
--   IF test_user_id IS NULL THEN
--     RAISE EXCEPTION 'No users found in auth.users table';
--   END IF;

-- 1. 성공 알림 (Success)
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at) 
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1), -- 첫 번째 사용자
  '새로운 고객이 추가되었습니다',
  '홍길동 고객이 데이터베이스에 성공적으로 등록되었습니다.',
  'success',
  false,
  '{"customer_id": "12345", "customer_name": "홍길동", "source": "web_form"}'::jsonb,
  NOW() - INTERVAL '5 minutes'
);

-- 2. 정보 알림 (Info)
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '새로운 리드가 배정되었습니다',
  '김영희 리드가 귀하에게 배정되었습니다. 24시간 이내에 연락해주세요.',
  'info',
  false,
  '{"lead_id": "67890", "lead_name": "김영희", "priority": "high"}'::jsonb,
  NOW() - INTERVAL '1 hour'
);

-- 3. 경고 알림 (Warning)
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '작업 마감일이 임박했습니다',
  '제안서 작성 작업의 마감일이 내일입니다. 완료 상태를 확인해주세요.',
  'warning',
  false,
  '{"task_id": "task_001", "task_name": "제안서 작성", "due_date": "2025-10-08"}'::jsonb,
  NOW() - INTERVAL '3 hours'
);

-- 4. 에러 알림 (Error)
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '이메일 전송 실패',
  '박철수 고객에게 발송한 이메일이 반송되었습니다. 이메일 주소를 확인해주세요.',
  'error',
  false,
  '{"email_id": "email_456", "recipient": "park@example.com", "error_code": "550"}'::jsonb,
  NOW() - INTERVAL '6 hours'
);

-- 5. 거래 업데이트 알림
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '거래가 협상 단계로 이동했습니다',
  '삼성전자 거래가 "협상" 단계로 전환되었습니다. 계약서를 준비해주세요.',
  'info',
  true,
  '{"deal_id": "deal_789", "company": "삼성전자", "stage": "negotiation", "value": 50000000}'::jsonb,
  NOW() - INTERVAL '1 day'
);

-- 6. 시스템 알림
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '시스템 업데이트 예정',
  '10월 10일 02:00~04:00 사이에 시스템 점검이 예정되어 있습니다.',
  'warning',
  true,
  '{"maintenance_date": "2025-10-10", "maintenance_time": "02:00-04:00", "type": "scheduled"}'::jsonb,
  NOW() - INTERVAL '2 days'
);

-- 7. 작업 완료 알림
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '작업이 완료되었습니다',
  '이정민님이 "고객 미팅" 작업을 완료했습니다.',
  'success',
  true,
  '{"task_id": "task_002", "task_name": "고객 미팅", "completed_by": "이정민"}'::jsonb,
  NOW() - INTERVAL '3 days'
);

-- 8. 새 메시지 알림
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '새 메시지가 도착했습니다',
  '최수진 고객이 문의 메시지를 보냈습니다. 확인해주세요.',
  'info',
  false,
  '{"message_id": "msg_999", "sender": "최수진", "subject": "제품 문의"}'::jsonb,
  NOW() - INTERVAL '30 minutes'
);

-- 9. 할당량 경고
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '월간 목표 달성률 80% 도달',
  '이번 달 매출 목표의 80%를 달성했습니다. 목표까지 2천만원 남았습니다!',
  'success',
  false,
  '{"target_amount": 100000000, "current_amount": 80000000, "percentage": 80}'::jsonb,
  NOW() - INTERVAL '2 hours'
);

-- 10. 팀 공지
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '팀 미팅 일정 안내',
  '매주 월요일 오전 10시 팀 미팅이 예정되어 있습니다. Zoom 링크를 확인해주세요.',
  'info',
  true,
  '{"meeting_day": "Monday", "meeting_time": "10:00", "platform": "Zoom"}'::jsonb,
  NOW() - INTERVAL '5 days'
);

-- 11. 보안 알림
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '새로운 위치에서 로그인 감지',
  '서울에서 귀하의 계정으로 로그인이 감지되었습니다. 본인이 아닌 경우 즉시 비밀번호를 변경하세요.',
  'warning',
  false,
  '{"location": "Seoul, Korea", "ip_address": "123.456.789.012", "timestamp": "2025-10-07 14:30:00"}'::jsonb,
  NOW() - INTERVAL '10 minutes'
);

-- 12. 계약 갱신 알림
INSERT INTO notification_feed (user_id, title, message, type, is_read, metadata, created_at)
VALUES 
(
  (SELECT id FROM auth.users LIMIT 1),
  '계약 갱신 기한이 다가옵니다',
  'LG유플러스와의 계약이 30일 후 만료됩니다. 갱신 협의를 시작하세요.',
  'warning',
  false,
  '{"contract_id": "contract_555", "company": "LG유플러스", "expiry_date": "2025-11-07"}'::jsonb,
  NOW() - INTERVAL '4 hours'
);

-- END;
-- $$;

-- ============================================================================
-- 알림 조회 쿼리 예제
-- ============================================================================

-- 읽지 않은 알림 조회
-- SELECT * FROM notification_feed 
-- WHERE user_id = 'YOUR_USER_ID' 
-- AND is_read = false 
-- ORDER BY created_at DESC;

-- 타입별 알림 조회
-- SELECT type, COUNT(*) as count 
-- FROM notification_feed 
-- WHERE user_id = 'YOUR_USER_ID' 
-- GROUP BY type;

-- 최근 24시간 내 알림 조회
-- SELECT * FROM notification_feed 
-- WHERE user_id = 'YOUR_USER_ID' 
-- AND created_at >= NOW() - INTERVAL '24 hours'
-- ORDER BY created_at DESC;

-- ============================================================================
-- 알림 업데이트 쿼리 예제
-- ============================================================================

-- 모든 알림을 읽음으로 표시
-- UPDATE notification_feed 
-- SET is_read = true, updated_at = NOW()
-- WHERE user_id = 'YOUR_USER_ID' 
-- AND is_read = false;

-- 특정 알림 읽음 표시
-- UPDATE notification_feed 
-- SET is_read = true, updated_at = NOW()
-- WHERE id = 'NOTIFICATION_ID';

-- ============================================================================
-- 알림 삭제 쿼리 예제
-- ============================================================================

-- 오래된 읽은 알림 삭제 (30일 이상)
-- DELETE FROM notification_feed 
-- WHERE user_id = 'YOUR_USER_ID' 
-- AND is_read = true 
-- AND created_at < NOW() - INTERVAL '30 days';
