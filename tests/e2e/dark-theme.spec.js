const fs = require('fs');
const path = require('path');
const { test, expect } = require('@playwright/test');

const TARGET_URL = process.env.TARGET_URL || 'https://localhost:7065/main-dashboard';
const SCREENSHOT_PATH = path.join('test-results', 'dark-theme', 'dashboard-dark.png');

async function ensureDirFor(filePath) {
  await fs.promises.mkdir(path.dirname(filePath), { recursive: true });
}

test.describe('Dark theme regression', () => {
  test('toggles dashboard into dark mode without losing tokens', async ({ page }) => {
    await ensureDirFor(SCREENSHOT_PATH);

    try {
      await page.goto(TARGET_URL, { waitUntil: 'networkidle' });
    } catch (error) {
      throw new Error(
        `Failed to load ${TARGET_URL}. Ensure the NexaCRM WebServer is running locally. Original error: ${error.message}`
      );
    }

    await page.waitForSelector('[data-theme-toggle]', { timeout: 15000 });
    await page.waitForSelector('.dashboard-card', { timeout: 15000 });

    await page.locator('[data-theme-toggle]').click();
    await page.waitForTimeout(600);

    const bodyTheme = await page.evaluate(() => {
      const root = document.documentElement.getAttribute('data-theme');
      const body = document.body.getAttribute('data-theme');
      return body || root || '';
    });
    const firstCardRadius = await page.locator('.dashboard-card').first().evaluate(el => {
      const styles = getComputedStyle(el);
      return styles.getPropertyValue('border-radius');
    });
    const firstCardBg = await page.locator('.dashboard-card').first().evaluate(el => {
      const styles = getComputedStyle(el);
      return styles.getPropertyValue('background-color');
    });

    expect(bodyTheme).toBe('dark');
    expect(firstCardRadius).not.toBe('');
    expect(firstCardBg).not.toBe('');

    await page.screenshot({ path: SCREENSHOT_PATH, fullPage: true });
    await test.info().attach('dark-theme-dashboard', {
      path: SCREENSHOT_PATH,
      contentType: 'image/png'
    });
  });
});
