Pull-to-Refresh — Developer Notes
=================================

Quick notes for developers and designers to test the pull-to-refresh indicator and live CSS updates during development.

1) Debug flag (auto-enabled in Development)
------------------------------------------
- When the app runs in the Development environment the host page injects a small script which sets:

  window.__NEXACRM_DEBUG__ = true

  This enables the pull-to-refresh indicator on touch devices so you can test the visuals without extra toggles.

2) Triggering the refresh manually (for testing)
------------------------------------------------
- From the browser console you can emulate the pull-to-refresh event without touch:

  window.dispatchEvent(new CustomEvent('nexacrm:pullToRefresh'))

  The app should receive the event (via the Blazor interop module) and perform any soft refresh logic, then dispatch
  'nexacrm:pullToRefreshComplete' when finished. The indicator will show a check animation then hide.

3) Live CSS sync during dev
---------------------------
- Run the CSS watcher to copy RCL CSS into the WebServer wwwroot so static file hot reload picks up changes quickly:

  npm run dev:watch:css

  (This uses chokidar-cli if installed; it falls back to a small Node watcher if chokidar is not available.)

4) How to test end-to-end
-------------------------
- Start the server with hot-reload in the foreground so you can see logs:

  dotnet watch run --project src/NexaCRM.WebServer/NexaCRM.WebServer.csproj

- Open the site in a mobile device or emulator (or use Chrome DevTools device toolbar). Pull down to trigger the visual indicator,
  or dispatch the event from the console as shown above.

5) Notes & troubleshooting
--------------------------
- If you see static web asset conflicts (build errors complaining about '_content/...'), ensure you haven't manually copied RCL
  static files into the WebServer _content folder. The watcher copies to wwwroot/css only and should not duplicate the RCL static assets.

6) Want the indicator visible outside Development?
--------------------------------------------------
- The indicator is gated by `window.__NEXACRM_DEBUG__`. You can temporarily set this in the console, but avoid committing changes that
  expose debug UI in production.

That's it — tweak visuals in `src/NexaCRM.UI/wwwroot/js/interactions.js` and use the watcher + dotnet watch for fast feedback.

Quick file-preview examples
--------------------------
If you installed the recommended dev packages, you can test quick previews in the browser console or in a small HTML test page.

1) Image preview with FilePond (quick console setup)

```javascript
// Create a basic FilePond input dynamically (for quick designer checks)
const input = document.createElement('input');
input.type = 'file';
document.body.appendChild(input);
FilePond.create(input, { allowMultiple: false });

// To add image preview plugin (already installed), ensure it's registered:
// FilePond.registerPlugin(FilePondPluginImagePreview);
```

2) PDF preview with pdfjs-dist (console)

```javascript
// Minimal PDF render to a canvas
import('pdfjs-dist/build/pdf').then(pdfjs => {
  pdfjs.GlobalWorkerOptions.workerSrc = '/node_modules/pdfjs-dist/build/pdf.worker.js';
  pdfjs.getDocument('/path/to/file.pdf').promise.then(doc => {
    doc.getPage(1).then(page => {
      const viewport = page.getViewport({ scale: 1.0 });
      const canvas = document.createElement('canvas');
      canvas.width = viewport.width; canvas.height = viewport.height;
      document.body.appendChild(canvas);
      page.render({ canvasContext: canvas.getContext('2d'), viewport });
    });
  });
});
```

3) DOCX -> HTML preview with Mammoth (node or browser bundler)

```javascript
// In node or a bundler that supports Mammoth
const mammoth = require('mammoth');
mammoth.convertToHtml({path: '/path/to/file.docx'}).then(result => {
  document.body.innerHTML = result.value;
});
```

These snippets are intentionally minimal — if you want, I can add a small `tools/preview-demo/` folder with ready-to-open HTML demo pages that load the installed packages for designers to drag/drop files and preview them quickly.
