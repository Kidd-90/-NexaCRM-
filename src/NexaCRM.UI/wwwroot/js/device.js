(function () {
    const global = window;
    const existing = global.deviceInfo;
    if (existing && existing._nexacrm_init) {
        return;
    }

    const PLATFORM = {
        DESKTOP: "desktop",
        ANDROID: "android",
        IOS: "ios"
    };

    let cachedPlatform = null;

    const normalize = (value) =>
        (value || "").toString().trim().toLowerCase();

    const parsePlatformFromUserAgentData = () => {
        try {
            const uaData = navigator.userAgentData;
            if (!uaData) {
                return null;
            }

            const platform = normalize(uaData.platform);
            if (platform.includes("android")) {
                return PLATFORM.ANDROID;
            }

            if (platform.includes("ios") || (platform.includes("mac") && uaData.mobile)) {
                return PLATFORM.IOS;
            }

            if (uaData.mobile === true) {
                const brands = Array.isArray(uaData.brands) ? uaData.brands : [];
                const brandNames = brands.map((brand) => normalize(brand.brand));
                if (brandNames.some((name) => name.includes("android"))) {
                    return PLATFORM.ANDROID;
                }

                if (brandNames.some((name) => name.includes("iphone") || name.includes("ipad"))) {
                    return PLATFORM.IOS;
                }
            }
        } catch (error) {
            try { console.warn("[device.js] Failed to read userAgentData", error); } catch (_) { /* ignore */ }
        }

        return null;
    };

    const parsePlatformFromUserAgent = () => {
        try {
            const ua = normalize(navigator.userAgent || navigator.vendor || global.opera);
            if (!ua) {
                return null;
            }

            if (ua.includes("android")) {
                return PLATFORM.ANDROID;
            }

            if (/iphone|ipad|ipod/.test(ua)) {
                return PLATFORM.IOS;
            }

            if (ua.includes("macintosh") && typeof navigator.maxTouchPoints === "number" && navigator.maxTouchPoints > 1) {
                return PLATFORM.IOS;
            }
        } catch (error) {
            try { console.warn("[device.js] Failed to inspect userAgent", error); } catch (_) { /* ignore */ }
        }

        return null;
    };

    const determinePlatform = () => {
        const fromUaData = parsePlatformFromUserAgentData();
        if (fromUaData) {
            return fromUaData;
        }

        const fromUa = parsePlatformFromUserAgent();
        if (fromUa) {
            return fromUa;
        }

        return PLATFORM.DESKTOP;
    };

    const isMobilePlatform = (platform) => platform === PLATFORM.ANDROID || platform === PLATFORM.IOS;

    const updateDocumentState = (platform) => {
        const root = document.documentElement;
        const body = document.body;
        const isMobile = isMobilePlatform(platform);

        if (root) {
            try {
                root.setAttribute("data-platform", platform);
                root.classList.toggle("platform-mobile", isMobile);
                root.classList.toggle("platform-desktop", !isMobile);
            } catch (error) {
                try { console.warn("[device.js] Failed to decorate <html>", error); } catch (_) { /* ignore */ }
            }
        }

        const applyBodyState = () => {
            if (!document.body) {
                return false;
            }

            try {
                document.body.setAttribute("data-platform", platform);
                document.body.classList.toggle("platform-mobile", isMobile);
                document.body.classList.toggle("platform-desktop", !isMobile);
            } catch (error) {
                try { console.warn("[device.js] Failed to decorate <body>", error); } catch (_) { /* ignore */ }
            }

            try {
                document.querySelectorAll(".page").forEach((page) => {
                    if (!(page instanceof HTMLElement)) {
                        return;
                    }

                    if (isMobile) {
                        page.classList.add("mobile-layout");
                    } else {
                        page.classList.remove("mobile-layout");
                    }
                });
            } catch (error) {
                try { console.warn("[device.js] Failed to adjust page containers", error); } catch (_) { /* ignore */ }
            }

            return true;
        };

        if (!applyBodyState()) {
            document.addEventListener("DOMContentLoaded", () => applyBodyState(), { once: true });
        }
    };

    const refreshPlatformState = () => {
        cachedPlatform = determinePlatform();
        updateDocumentState(cachedPlatform);
        return cachedPlatform;
    };

    const getPlatform = () => {
        if (!cachedPlatform) {
            cachedPlatform = determinePlatform();
        }

        return cachedPlatform;
    };

    const device = {
        getPlatform,
        isAndroid: () => getPlatform() === PLATFORM.ANDROID,
        isIOS: () => getPlatform() === PLATFORM.IOS,
        isMobile: () => isMobilePlatform(getPlatform()),
        refreshPlatformState
    };

    device._nexacrm_init = true;

    global.deviceInfo = Object.assign(global.deviceInfo || {}, device);
    global.deviceInterop = Object.assign(global.deviceInterop || {}, {
        getPlatform: () => device.getPlatform(),
        isAndroid: () => device.isAndroid(),
        isIOS: () => device.isIOS(),
        isIos: () => device.isIOS(),
        isMobile: () => device.isMobile(),
        refreshPlatformState: () => device.refreshPlatformState()
    });

    const scheduleRefresh = () => {
        let timeoutId = null;
        const trigger = () => {
            if (timeoutId) {
                global.clearTimeout(timeoutId);
            }

            timeoutId = global.setTimeout(() => {
                timeoutId = null;
                try {
                    refreshPlatformState();
                } catch (error) {
                    try { console.warn("[device.js] Failed to refresh platform state", error); } catch (_) { /* ignore */ }
                }
            }, 150);
        };

        global.addEventListener("resize", trigger);
        global.addEventListener("orientationchange", trigger);
    };

    refreshPlatformState();
    scheduleRefresh();
})();
