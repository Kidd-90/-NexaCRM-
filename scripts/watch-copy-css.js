const fs = require('fs');
const fsp = fs.promises;
const path = require('path');

// Standardized source and destination paths
const srcDir = path.resolve(__dirname, '..', 'src', 'NexaCRM.UI', 'wwwroot', 'css');
const destDir = path.resolve(__dirname, '..', 'src', 'NexaCRM.WebServer', 'wwwroot', 'css');

const DEBOUNCE_MS = 100;
let timers = new Map();

function log(...args) { console.log('[css-sync]', ...args); }
function err(...args) { console.error('[css-sync]', ...args); }

async function ensureDir(dir) {
  try {
    await fsp.mkdir(dir, { recursive: true });
  } catch (e) {
    // ignore
  }
}

async function copyFileAtomic(src, dest) {
  await ensureDir(path.dirname(dest));
  await fsp.copyFile(src, dest);
}

async function removeIfExists(p) {
  try {
    const stat = await fsp.lstat(p);
    if (stat.isDirectory()) {
      await fsp.rm(p, { recursive: true, force: true });
    } else {
      await fsp.unlink(p);
    }
  } catch (e) {
    // ignore missing
  }
}

async function copyDirRecursive(s, d) {
  // ensure src exists
  try {
    const stat = await fsp.stat(s);
    if (!stat.isDirectory()) return;
  } catch (e) {
    return;
  }
  await ensureDir(d);
  const entries = await fsp.readdir(s, { withFileTypes: true });
  for (const entry of entries) {
    const srcPath = path.join(s, entry.name);
    const destPath = path.join(d, entry.name);
    if (entry.isDirectory()) {
      await copyDirRecursive(srcPath, destPath);
    } else if (entry.isFile()) {
      await copyFileAtomic(srcPath, destPath);
    }
  }
}

async function removeExtraneous(s, d) {
  // Remove files/dirs in dest that don't exist in src
  try {
    const entries = await fsp.readdir(d, { withFileTypes: true });
    for (const entry of entries) {
      const destPath = path.join(d, entry.name);
      const srcPath = path.join(s, entry.name);
      try {
        await fsp.access(srcPath);
        if (entry.isDirectory()) {
          await removeExtraneous(srcPath, destPath);
        }
      } catch (e) {
        // src missing -> remove dest
        await removeIfExists(destPath);
        log('removed', path.relative(destDir, destPath));
      }
    }
  } catch (e) {
    // dest may not exist yet
  }
}

async function syncAll() {
  try {
    await copyDirRecursive(srcDir, destDir);
    await removeExtraneous(srcDir, destDir);
    log('initial sync complete');
  } catch (e) {
    err('sync failed', e);
  }
}

function schedule(fn, key = 'global', ms = DEBOUNCE_MS) {
  if (timers.has(key)) clearTimeout(timers.get(key));
  timers.set(key, setTimeout(() => { timers.delete(key); fn(); }, ms));
}

function relToSrc(fullPath) {
  return path.relative(srcDir, fullPath);
}

async function handleAddChange(fullPath) {
  const rel = relToSrc(fullPath);
  const destPath = path.join(destDir, rel);
  try {
    const stat = await fsp.stat(fullPath);
    if (stat.isDirectory()) {
      await ensureDir(destPath);
      log('dir added', rel);
    } else {
      await copyFileAtomic(fullPath, destPath);
      log('copied', rel);
    }
  } catch (e) {
    err('handle add/change failed', rel, e);
  }
}

async function handleUnlink(fullPath) {
  const rel = relToSrc(fullPath);
  const destPath = path.join(destDir, rel);
  try {
    await removeIfExists(destPath);
    log('removed', rel);
  } catch (e) {
    err('handle unlink failed', rel, e);
  }
}

async function startWatcher() {
  // Initial sync
  await syncAll();

  // Try to use chokidar if available for robust watching
  let chokidar;
  try {
    chokidar = require('chokidar');
  } catch (e) {
    chokidar = null;
  }

  if (chokidar) {
    log('using chokidar for file watching');
    const watcher = chokidar.watch(srcDir, { ignoreInitial: true, persistent: true, depth: 10 });
    watcher.on('add', p => schedule(() => handleAddChange(p), p));
    watcher.on('change', p => schedule(() => handleAddChange(p), p));
    watcher.on('addDir', p => schedule(() => handleAddChange(p), p));
    watcher.on('unlink', p => schedule(() => handleUnlink(p), p));
    watcher.on('unlinkDir', p => schedule(() => handleUnlink(p), p));
    watcher.on('error', e => err('watcher error', e));

    process.on('SIGINT', async () => {
      log('stopping watcher');
      await watcher.close();
      process.exit(0);
    });
  } else {
    // Fallback to fs.watch with periodic full sync debounce
    log('chokidar not available, falling back to fs.watch (less reliable)');
    try {
      await ensureDir(srcDir);
    } catch (e) {}
    try {
      const w = fs.watch(srcDir, { recursive: true }, (eventType, filename) => {
        if (!filename) {
          schedule(syncAll, 'global');
          return;
        }
        const full = path.join(srcDir, filename);
        schedule(() => {
          // attempt to stat to determine if exists
          fsp.stat(full).then(stat => handleAddChange(full)).catch(() => handleUnlink(full));
        }, filename);
      });

      process.on('SIGINT', () => { log('stopping fallback watcher'); w.close(); process.exit(0); });
    } catch (e) {
      err('fs.watch failed, falling back to polling sync', e);
      // Polling fallback
      setInterval(() => schedule(syncAll, 'global'), 2000);
    }
  }
}

startWatcher().catch(e => { err('watcher failed to start', e); process.exit(1); });

