-- Update password hashes to BCrypt format
UPDATE user_infos SET password_hash = CASE
  WHEN user_cuid = 'cuid_manager_001' THEN '$2a$11$ziaUgFjuZiWWcC.CR4PVmOX0VfdvhQxZtWBU0nFDbH1.dw0Zc94qi'
  WHEN user_cuid = 'cuid_sales_001' THEN '$2a$11$MUiAIJeVaHBorMtbDz3qbOz83Dl.BqsnEGqeytDzwwqKuRHOidjdm'
  WHEN user_cuid = 'cuid_develop_001' THEN '$2a$11$emotXHMhO4ZjekujhtXIquEOmvVTj5OMGlmayPVJdV9AIfGHtPrAq'
  ELSE password_hash
END;
