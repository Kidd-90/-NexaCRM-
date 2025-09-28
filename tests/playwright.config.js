const { devices } = require('@playwright/test');

module.exports = {
  timeout: 120_000,
  testDir: './e2e',
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:60743',
    headless: true,
    viewport: { width: 1366, height: 768 },
    ignoreHTTPSErrors: true,
  },
};
