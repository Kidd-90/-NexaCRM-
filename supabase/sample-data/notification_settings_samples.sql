-- Notification Settings 샘플 데이터 생성 스크립트
-- 사용법: Supabase SQL Editor에서 실행
-- 주의: {your-user-id}를 실제 사용자 ID로 교체하세요

-- 1. 현재 로그인한 사용자의 알림 설정 확인
SELECT * FROM notification_settings WHERE user_id = '{your-user-id}';

-- 2. 알림 설정이 없다면 기본값으로 생성 (선택사항 - 앱에서 자동 생성됨)
-- INSERT INTO notification_settings (
--   user_id,
--   new_lead_created,
--   lead_status_updated,
--   deal_stage_changed,
--   deal_value_updated,
--   new_task_assigned,
--   task_due_date_reminder,
--   email_notifications,
--   in_app_notifications,
--   push_notifications
-- ) VALUES (
--   '{your-user-id}',
--   true,
--   true,
--   true,
--   true,
--   true,
--   true,
--   true,
--   true,
--   false
-- )
-- ON CONFLICT (user_id) DO NOTHING;

-- 3. 기존 설정 업데이트 (테스트용)
-- UPDATE notification_settings
-- SET 
--   new_lead_created = true,
--   lead_status_updated = false,
--   deal_stage_changed = true,
--   email_notifications = true,
--   updated_at = NOW()
-- WHERE user_id = '{your-user-id}';

-- 4. 모든 사용자의 알림 설정 조회
SELECT 
  ns.user_id,
  u.email,
  ns.email_notifications,
  ns.in_app_notifications,
  ns.push_notifications,
  ns.updated_at
FROM notification_settings ns
LEFT JOIN auth.users u ON u.id = ns.user_id
ORDER BY ns.updated_at DESC;
