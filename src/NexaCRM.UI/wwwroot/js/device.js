(function(){
    if (window.deviceInfo && window.deviceInfo._nexacrm_init) {
        return;
    }

    const _device = {
        isMobile: function () {
            if (window.matchMedia) {
                if (window.matchMedia('(max-width: 767px)').matches) {
                    return true;
                }
            }
            const ua = navigator.userAgent || navigator.vendor || window.opera;
            return /android|iPad|iPhone|iPod/i.test(ua);
        }
    };

    _device._nexacrm_init = true;
    window.deviceInfo = Object.assign(window.deviceInfo || {}, _device);
})();
