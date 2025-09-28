(function(){
    if (window.cultureHelpers && window.cultureHelpers._nexacrm_init) return;

    const _helpers = {
        setCultureCookie: function(value) {
            document.cookie = ".AspNetCore.Culture=" + value + "; path=/; samesite=strict";
        },
        getCultureCookie: function() {
            const name = ".AspNetCore.Culture=";
            const decodedCookie = decodeURIComponent(document.cookie);
            const ca = decodedCookie.split(';');
            for(let i = 0; i <ca.length; i++) {
                let c = ca[i];
                while (c.charAt(0) == ' ') {
                    c = c.substring(1);
                }
                if (c.indexOf(name) == 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return null;
        }
    };

    _helpers._nexacrm_init = true;
    window.cultureHelpers = Object.assign(window.cultureHelpers || {}, _helpers);
})();
