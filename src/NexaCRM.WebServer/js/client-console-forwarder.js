// Client console forwarder - sends console messages and unhandled errors to server for debugging
(function () {
    if (window.clientConsoleForwarder && window.clientConsoleForwarder._nexacrm_init) return;
    // Default to relative endpoint (good for production). In local development
    // prefer the WebServer process which listens on http:5065 and https:7065.
    let ENDPOINT = '/client-logs';
    try {
        if (location && location.hostname === 'localhost') {
            // Use https port when the page is served over https to avoid mixed-content
            if (location.protocol === 'https:') {
                ENDPOINT = 'https://localhost:7065/client-logs';
            } else {
                ENDPOINT = 'http://localhost:5065/client-logs';
            }
        }
    } catch (e) {
        // ignore - keep relative endpoint
    }

    function sendLog(payload) {
        try {
            const body = JSON.stringify(payload);
            // Try sendBeacon first (non-blocking)
            if (navigator.sendBeacon) {
                const blob = new Blob([body], { type: 'application/json' });
                navigator.sendBeacon(ENDPOINT, blob);
                return;
            }

            // Fallback to fetch (fire-and-forget)
            fetch(ENDPOINT, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body,
                keepalive: true
            }).catch(() => { /* ignore */ });
        } catch (e) {
            // ignore
        }
    }

    function formatArgs(args) {
        try {
            return args.map(a => {
                if (a instanceof Error) return { message: a.message, stack: a.stack };
                if (typeof a === 'object') return JSON.parse(JSON.stringify(a));
                return String(a);
            });
        } catch (e) {
            return args.map(a => String(a));
        }
    }

    const origConsole = {
        log: console.log.bind(console),
        info: console.info.bind(console),
        warn: console.warn.bind(console),
        error: console.error.bind(console)
    };

    ['log', 'info', 'warn', 'error'].forEach(level => {
        console[level] = function (...args) {
            try {
                const payload = {
                    level,
                    message: args.map(a => (typeof a === 'string' ? a : (a && a.message) || JSON.stringify(a))).join(' '),
                    args: formatArgs(args),
                    url: location.href,
                    userAgent: navigator.userAgent,
                    ts: new Date().toISOString()
                };
                sendLog(payload);
            } catch (e) {
                // ignore
            }
            origConsole[level].apply(console, args);
        };
    });

    window.addEventListener('error', (ev) => {
        try {
            const payload = {
                level: 'error',
                message: ev.message || String(ev.error || ev),
                args: [{ message: ev.error?.message, stack: ev.error?.stack }],
                url: location.href,
                userAgent: navigator.userAgent,
                ts: new Date().toISOString()
            };
            sendLog(payload);
        } catch (e) { }
    });

    window.addEventListener('unhandledrejection', (ev) => {
        try {
            const reason = ev.reason;
            const payload = {
                level: 'error',
                message: reason?.message || String(reason),
                args: [{ message: reason?.message, stack: reason?.stack }],
                url: location.href,
                userAgent: navigator.userAgent,
                ts: new Date().toISOString()
            };
            sendLog(payload);
        } catch (e) { }
    });
    // mark initialized to avoid duplicate monkey-patching of console
    try { window.clientConsoleForwarder = window.clientConsoleForwarder || {}; window.clientConsoleForwarder._nexacrm_init = true; } catch (e) { }
})();
