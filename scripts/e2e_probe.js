const { chromium } = require('playwright');

(async () => {
  const base = process.env.BASE_URL || 'http://localhost:5065';
  const url = new URL(base);
  const cookie = Buffer.from(JSON.stringify({ user: { id: 'probe-id', email: 'probe@example.com' }, roles: ['user'] })).toString('base64');

  const browser = await chromium.launch();
  const context = await browser.newContext();
  await context.addCookies([{ name: 'e2e_session', value: cookie, domain: url.hostname, path: '/' }]);
  const page = await context.newPage();
  const r = await page.goto(base, { waitUntil: 'networkidle' });
  console.log('status', r.status());
  await browser.close();
})();
