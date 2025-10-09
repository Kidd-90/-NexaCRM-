(function(){
    if (window.deviceInfo && window.deviceInfo._nexacrm_init) {
        return;
    }

    const _device = {
        isMobile: function () {
            const ua = navigator.userAgent || navigator.vendor || window.opera;
            if (/android|iPad|iPhone|iPod/i.test(ua)) {
                return true;
            }

            if (window.matchMedia) {
                if (window.matchMedia('(max-width: 767px)').matches) {
                    return true;
                }
            }

            return false;
        }
    };

    _device._nexacrm_init = true;
    window.deviceInfo = Object.assign(window.deviceInfo || {}, _device);
    window.deviceInterop = Object.assign(window.deviceInterop || {}, {
        isMobile: function () {
            return window.deviceInfo && typeof window.deviceInfo.isMobile === "function"
                ? window.deviceInfo.isMobile()
                : false;
        }
    });
})();
