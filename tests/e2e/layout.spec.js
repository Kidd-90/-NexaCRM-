const { test, expect } = require('@playwright/test');

// pages to test - trimmed set for initial run, can be expanded
const PAGES = [
  '/',
  '/main-dashboard',
  '/customer-management',
  '/contacts',
  '/db/customer/my-list',
  '/db/customer/all',
  '/reports-page',
  '/sales-calendar',
  '/sms/history',
  '/settings/security'
];

// helper to read CSS variable from element
async function cssVar(page, selector, varName) {
  return await page.$eval(selector, (el, v) => {
    return getComputedStyle(el).getPropertyValue(v).trim();
  }, varName);
}

// Capture fetch/XHR posts to console-forwarder endpoint (heuristic)
async function captureForwarderPosts(page) {
  const posts = [];
  await page.route('**/*', route => route.continue());
  page.on('request', req => {
    if (req.method() === 'POST' && req.url().includes('/console-forwarder')) {
      posts.push({ url: req.url(), postData: req.postData() });
    }
  });
  return posts;
}

for (const path of PAGES) {
  test.describe(`Layout tests: ${path}`, () => {
  test(`header measurement and offsets for ${path}`, async ({ page, baseURL }) => {
  const defaultBase = process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:60743';
  const base = baseURL || defaultBase;
  const url = new URL(path, base).toString();

        // helper: ensure the test browser context is authenticated by
        // calling the development-only signin endpoint and seeding
        // localStorage before navigation. If the server-side signin
        // fails, fall back to seeding localStorage so client-side
        // components that rely on it can render.
        async function ensureAuthenticated() {
          const username = process.env.E2E_TEST_USERNAME || process.env.E2E_USERNAME || 'manager';
          const password = process.env.E2E_TEST_PASSWORD || process.env.E2E_PASSWORD || 'password';
          let roles = ['user'];
          try {
            const resp = await page.request.post(new URL('/test/signin', base).toString(), { data: { username, password } });
            if (resp.ok()) {
              try {
                const body = await resp.json();
                if (body && body.success) {
                  roles = body.roles || roles;
                  // if the server returned a mock Supabase session, persist it into the
                  // browser localStorage under the key that SupabaseSessionPersistence
                  // expects so client-side AuthenticationStateProvider will pick it up.
                  if (body.session) {
                    const raw = body.session;
                    // Normalize common snake_case or camelCase shapes into the
                    // PascalCase shape expected by the .NET Supabase Session type.
                    const sess = {
                      AccessToken: raw.access_token || raw.AccessToken || raw.accessToken || null,
                      TokenType: raw.token_type || raw.TokenType || raw.tokenType || null,
                      ExpiresIn: raw.expires_in || raw.ExpiresIn || raw.expiresIn || null,
                      ExpiresAt: raw.expires_at || raw.ExpiresAt || raw.expiresAt || null,
                      RefreshToken: raw.refresh_token || raw.RefreshToken || raw.refreshToken || null,
                      ProviderToken: raw.provider_token || raw.ProviderToken || raw.providerToken || null,
                      ProviderRefreshToken: raw.provider_refresh_token || raw.ProviderRefreshToken || raw.providerRefreshToken || null,
                      User: raw.user ? {
                        Id: raw.user.id || raw.user.Id || raw.user.ID || null,
                        Email: raw.user.email || raw.user.Email || null,
                        Aud: raw.user.aud || raw.user.Aud || null,
                        Role: raw.user.role || raw.user.Role || null,
                        ConfirmedAt: raw.user.confirmed_at || raw.user.ConfirmedAt || null
                      } : null
                    };
                    await page.addInitScript(s => {
                      try {
                        localStorage.setItem('nexacrm.supabase.session', JSON.stringify(s));
                      } catch (e) {}
                    }, sess);

                    // Also set an e2e_session cookie (base64-encoded JSON) so the
                    // server development middleware can pick up authentication
                    // state for Blazor Server circuits. The cookie will be set in
                    // the browser context and sent on subsequent navigations.
                    try {
                      const cookiePayload = JSON.stringify({ user: sess.User, roles });
                      const encoded = Buffer.from(cookiePayload, 'utf8').toString('base64');
                      await page.context().addCookies([{
                        name: 'e2e_session',
                        value: encoded,
                        domain: new URL(base).hostname,
                        path: '/',
                        httpOnly: false,
                        secure: false
                      }]);
                    } catch (e) {
                      // ignore
                    }
                  }
                }
              } catch (e) {
                // ignore JSON parse errors
              }
            }
          } catch (e) {
            // endpoint may not be available — fall through to seeding localStorage
          }

          // Seed localStorage before any page scripts run
          await page.addInitScript(({ username, roles }) => {
            try {
              localStorage.setItem('username', username);
              localStorage.setItem('roles', JSON.stringify(roles));
              // if roles contains 'developer' set the isDeveloper flag
              if (Array.isArray(roles) && roles.indexOf('developer') !== -1) {
                localStorage.setItem('isDeveloper', 'true');
              }
            } catch (e) {}
          }, { username, roles });
        }

        // capture forwarder posts triggered during navigation/load
        const posts = [];
        const failedRequests = [];
        page.on('request', req => {
          if (req.method() === 'POST' && req.url().includes('/console-forwarder')) {
            posts.push({ url: req.url(), postData: req.postData() });
          }
        });
        page.on('requestfailed', req => {
          failedRequests.push({ url: req.url(), failure: req.failure()?.errorText });
        });
        const pageErrors = [];
        page.on('pageerror', err => { pageErrors.push(String(err)); });

        // ensure authentication in this browser context for protected routes
        await ensureAuthenticated();

        // navigate then wait for header to appear (app may show loader first)
        await page.goto(url, { waitUntil: 'networkidle' });

        // wait for layout stabilization: prefer the explicit layout.ready console
        // message, otherwise poll the exposed window.layoutInterop.isLayoutStable()
        let layoutReady = false;
        const consoleListener = msg => {
          try {
            if (msg.type() === 'info' && msg.text && msg.text().includes && msg.text().includes('[layout.ready]')) {
              layoutReady = true;
            }
          } catch (e) {}
        };
        page.on('console', consoleListener);

        // Poll the page for the layout stable flag with a timeout
        const maxWait = 5000;
        const start = Date.now();
        while (!layoutReady && Date.now() - start < maxWait) {
          // try evaluating the exposed helper
          try {
            const isStable = await page.evaluate(() => {
              try {
                if (window.layoutInterop && typeof window.layoutInterop.isLayoutStable === 'function') {
                  return window.layoutInterop.isLayoutStable();
                }
                return Boolean(window.__nexacrm_layout_stable);
              } catch (e) { return false; }
            });
            if (isStable) { layoutReady = true; break; }
          } catch (e) {}
          await page.waitForTimeout(120);
        }
        // cleanup listener
        page.off('console', consoleListener);

        // wait for the page header to be rendered by client app (may not exist on some pages)
        const headerSel = '.app-shell__page-header';
        try {
          await page.waitForSelector(headerSel, { timeout: 30000 });
        } catch (e) {
          // don't fail yet — we'll attempt CSS-variable based validation as a fallback
        }

        // give layout script a small amount of time to stabilize DOM/CSS vars
        await page.waitForTimeout(500);

        // capture console messages for debugging
        const consoleMsgs = [];
        page.on('console', msg => {
          try { consoleMsgs.push({ type: msg.type(), text: msg.text() }); } catch (e) {}
        });

  // content and nav selectors
  const contentSel = '.app-shell__content';
  const navRailSel = '.nav-rail';

  // try to locate header element; if absent, fall back to validating CSS vars and content margin
  const header = await page.$(headerSel);
  let headerRect = null;
  if (header) {
    // measure header height via getBoundingClientRect
    headerRect = await header.boundingBox();
    expect(headerRect.height).toBeGreaterThan(0);
  }

  // read CSS variables set by layout.js (if present)
  const cssHeight = await cssVar(page, ':root', '--app-shell-page-header-height');
  const cssOffset = await cssVar(page, ':root', '--app-shell-page-header-offset');
  const parsedHeight = cssHeight ? parseFloat(cssHeight) : null;
  const parsedOffset = cssOffset ? parseFloat(cssOffset) : null;

  // If header exists, sanity-check CSS vars against measured header
  if (headerRect && parsedHeight !== null) {
    expect(Math.abs(parsedHeight - headerRect.height)).toBeLessThanOrEqual(20);
  }

  if (parsedOffset !== null && (parsedHeight !== null || headerRect)) {
    const refHeight = parsedHeight !== null ? parsedHeight : (headerRect ? headerRect.height : 0);
    // offset should be >= header height and within header + 40px
    expect(parsedOffset).toBeGreaterThanOrEqual(refHeight);
    expect(parsedOffset - refHeight).toBeLessThanOrEqual(40);
  }

  // ensure content starts below header visually. If header isn't present, use CSS offset or margin-top as fallback.
  const content = await page.$(contentSel);
  expect(content, `content not found on ${url}`).not.toBeNull();
  const contentBox = await content.boundingBox();
  if (headerRect) {
    expect(contentBox.y).toBeGreaterThanOrEqual(headerRect.y + headerRect.height - 1);
  } else if (parsedOffset !== null) {
    // some pages render header via CSS vars only; validate content's computed margin-top
    const contentMarginTopPx = await page.$eval(contentSel, el => parseFloat(getComputedStyle(el).marginTop) || 0);
    expect(contentMarginTopPx).toBeGreaterThanOrEqual(parsedOffset - 2);
  }

      // check nav-rail overlap (content left edge should be >= nav-rail right edge)
      const nav = await page.$(navRailSel);
      if (nav) {
        const navBox = await nav.boundingBox();
        expect(contentBox.x).toBeGreaterThanOrEqual(navBox.x + navBox.width - 1);
      }

      // wait a bit for any async posts
      await page.waitForTimeout(300);

      // report at least that the test ran; allow zero posts but include any captured posts
      test.info().annotations.push({ type: 'posts', description: JSON.stringify(posts) });
    });
  });
}
