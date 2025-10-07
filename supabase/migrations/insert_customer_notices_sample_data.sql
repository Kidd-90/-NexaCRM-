-- Insert sample customer notices data
-- This script can be run independently to populate the customer_notices table with sample data

-- Clear existing data (optional - comment out if you want to keep existing data)
-- DELETE FROM customer_notices;

-- Insert sample notices with various categories and importance levels
INSERT INTO customer_notices (title, summary, content, category, importance, is_pinned, published_at) VALUES
(
  '시스템 업데이트 안내',
  'NexaCRM 시스템이 업데이트됩니다.',
  '2025년 10월 15일 오전 2시부터 4시까지 시스템 업데이트가 진행됩니다. 이 시간 동안 서비스 이용이 일시적으로 중단될 수 있습니다. 양해 부탁드립니다.',
  'Update',
  'Highlight',
  true,
  NOW()
),
(
  '보안 패치 적용 완료',
  '최신 보안 패치가 적용되었습니다.',
  '고객 데이터 보호를 위한 보안 패치가 성공적으로 적용되었습니다. 별도의 조치는 필요하지 않으며, 모든 사용자는 계속해서 안전하게 시스템을 이용하실 수 있습니다.',
  'Security',
  'Normal',
  false,
  NOW() - INTERVAL '1 day'
),
(
  '새로운 기능 출시: AI 리드 스코어링',
  'AI 기반 리드 스코어링 기능이 추가되었습니다.',
  '머신러닝을 활용한 AI 리드 스코어링 기능이 새롭게 추가되었습니다. 영업 대시보드에서 확인하실 수 있으며, 고객 전환 가능성을 자동으로 분석하여 우선순위를 제시합니다.',
  'Update',
  'Highlight',
  true,
  NOW() - INTERVAL '2 days'
),
(
  '정기 점검 일정 안내',
  '매월 첫째 주 일요일 정기 점검이 진행됩니다.',
  '서비스 품질 향상을 위해 매월 첫째 주 일요일 오전 2시부터 5시까지 정기 점검이 진행됩니다. 점검 시간 동안 일부 서비스 이용이 제한될 수 있습니다.',
  'Maintenance',
  'Normal',
  false,
  NOW() - INTERVAL '5 days'
),
(
  '개인정보 처리방침 업데이트',
  '개인정보 처리방침이 업데이트되었습니다.',
  '2025년 10월 1일부로 개인정보 처리방침이 업데이트되었습니다. 주요 변경 사항은 데이터 보관 기간 및 제3자 제공 범위에 관한 내용입니다. 자세한 내용은 고객센터를 참고해 주세요.',
  'Policy',
  'Normal',
  false,
  NOW() - INTERVAL '7 days'
),
(
  '프로모션: 연간 구독 20% 할인',
  '10월 한정 연간 구독 특별 할인 이벤트',
  '10월 한 달간 연간 구독 시 20% 할인 혜택을 제공합니다. 이 기회를 통해 NexaCRM의 모든 프리미엄 기능을 더욱 저렴하게 이용하세요. 자세한 내용은 영업팀에 문의해 주세요.',
  'Promotion',
  'Highlight',
  false,
  NOW() - INTERVAL '3 days'
),
(
  '고객센터 운영 시간 안내',
  '고객센터 운영 시간이 확대되었습니다.',
  '더 나은 고객 지원을 위해 고객센터 운영 시간이 평일 오전 9시부터 오후 8시까지로 확대되었습니다. 주말 및 공휴일은 오전 10시부터 오후 6시까지 운영됩니다.',
  'General',
  'Normal',
  false,
  NOW() - INTERVAL '10 days'
),
(
  '긴급 보안 업데이트 적용 필요',
  '중요한 보안 취약점이 발견되어 긴급 업데이트가 필요합니다.',
  '최근 발견된 보안 취약점으로 인해 모든 사용자는 즉시 시스템 업데이트를 적용해 주시기 바랍니다. 업데이트를 적용하지 않을 경우 데이터 유출의 위험이 있을 수 있습니다. 자세한 내용은 보안팀에 문의해 주세요.',
  'Security',
  'Critical',
  true,
  NOW() - INTERVAL '12 hours'
),
(
  '신규 모바일 앱 출시',
  'NexaCRM 모바일 앱이 출시되었습니다.',
  'iOS 및 Android에서 사용 가능한 NexaCRM 모바일 앱이 출시되었습니다. 이제 언제 어디서나 고객 정보를 조회하고 영업 활동을 관리할 수 있습니다. App Store와 Google Play에서 다운로드하세요.',
  'Update',
  'Highlight',
  false,
  NOW() - INTERVAL '4 days'
),
(
  '서비스 약관 변경 안내',
  '서비스 이용 약관이 변경됩니다.',
  '2025년 11월 1일부로 서비스 이용 약관이 일부 변경됩니다. 주요 변경 사항은 환불 정책 및 서비스 제공 범위에 관한 내용입니다. 변경된 약관은 홈페이지에서 확인하실 수 있습니다.',
  'Policy',
  'Normal',
  false,
  NOW() - INTERVAL '6 days'
),
(
  '블랙 프라이데이 특별 할인',
  '최대 50% 할인 혜택을 놓치지 마세요!',
  '11월 블랙 프라이데이를 맞아 모든 요금제에서 최대 50% 할인 혜택을 제공합니다. 이번 기회에 프리미엄 플랜으로 업그레이드하고 더 많은 기능을 경험해 보세요. 11월 30일까지 유효합니다.',
  'Promotion',
  'Critical',
  true,
  NOW() - INTERVAL '8 hours'
),
(
  '웨비나 안내: CRM 활용 전략',
  '효과적인 CRM 활용 방법을 알려드립니다.',
  '10월 20일 오후 3시에 "CRM을 활용한 영업 생산성 향상 전략" 웨비나가 개최됩니다. 업계 전문가의 노하우와 실전 팁을 공유하는 자리이니 많은 참여 바랍니다. 사전 등록은 홈페이지에서 가능합니다.',
  'General',
  'Normal',
  false,
  NOW() - INTERVAL '9 days'
);

-- Verify the inserted data
SELECT 
  id,
  title,
  category,
  importance,
  is_pinned,
  published_at
FROM customer_notices
ORDER BY is_pinned DESC, published_at DESC;
