const fs = require('fs');
const path = require('path');
const { test } = require('@playwright/test');

const TARGET_URL = process.env.TARGET_URL || 'https://localhost:7065';
const SCREENSHOT_PATH = path.join('test-results', 'design-check', 'home.png');

async function ensureDirFor(filePath) {
  await fs.promises.mkdir(path.dirname(filePath), { recursive: true });
}

test.describe('Design check - https://localhost:7065', () => {
  test('capture landing layout', async ({ page }) => {
    const consoleMessages = [];
    const pageErrors = [];

    page.on('console', message => {
      try {
        const type = message.type();
        if (type === 'warning' || type === 'error') {
          consoleMessages.push({ type, text: message.text() });
        }
      } catch (e) {
        // ignore formatting errors
      }
    });

    page.on('pageerror', error => {
      pageErrors.push(String(error));
    });

    await ensureDirFor(SCREENSHOT_PATH);

    try {
      await page.goto(TARGET_URL, { waitUntil: 'networkidle' });
    } catch (error) {
      throw new Error(
        `Failed to load ${TARGET_URL}. Ensure the NexaCRM WebServer is running locally. Original error: ${error.message}`
      );
    }

    await page.waitForLoadState('networkidle');
    await page.waitForSelector('#nexacrm-initial-loader', { state: 'hidden', timeout: 15000 }).catch(() => {});
    await page.waitForSelector('.mobile-login-card', { state: 'visible', timeout: 15000 });
    await page.waitForTimeout(500);

    await page.screenshot({ path: SCREENSHOT_PATH, fullPage: true });
    await test.info().attach('design-screenshot', {
      path: SCREENSHOT_PATH,
      contentType: 'image/png'
    });

    if (pageErrors.length) {
      test.info().annotations.push({
        type: 'warning',
        description: `Page errors encountered: ${pageErrors.join(' | ')}`
      });
    }

    if (consoleMessages.length) {
      const summary = consoleMessages.map(({ type, text }) => `[${type}] ${text}`).join('\n');
      test.info().annotations.push({
        type: 'warning',
        description: `Console issues detected:\n${summary}`
      });
    }
  });
});
