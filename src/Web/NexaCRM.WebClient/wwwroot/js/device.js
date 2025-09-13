window.deviceInfo = {
    isMobile: function () {
        if (window.matchMedia) {
            if (window.matchMedia('(max-width: 768px)').matches) {
                return true;
            }
        }
        const ua = navigator.userAgent || navigator.vendor || window.opera;
        return /android|iPad|iPhone|iPod/i.test(ua);
    }
};
