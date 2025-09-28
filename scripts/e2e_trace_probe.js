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
  console.log('host status', r.status());
  // Call /test/trace from the page context so cookies are sent
  const trace = await page.evaluate(async () => {
    try {
      const res = await fetch('/test/trace', { credentials: 'same-origin' });
      return await res.json();
    } catch (e) {
      return { error: e.message };
    }
  });
  console.log('trace', trace);
  await browser.close();
})();
