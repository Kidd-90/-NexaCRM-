// navigationStorageSync.js
// Helper to notify Blazor components when localStorage key changes in other tabs.
// Usage:
// window.navigationStorageSync.registerDotNet(dotNetRef, 'nexacrm.activeSubMenuIndex')
// window.navigationStorageSync.unregisterDotNet()

(function () {
  if (window.navigationStorageSync) return; // already loaded

  let dotNetRef = null;
  let keyToWatch = null;

  function onStorage(e) {
    try {
      if (!e) return;
      // only notify when the watched key changed
      if (!keyToWatch || e.key !== keyToWatch) return;
      // value may be null when removed
      const newValue = e.newValue;
      if (dotNetRef) {
        // call .NET with the new value (string or null)
        dotNetRef.invokeMethodAsync('OnExternalStorageChange', newValue).catch(err => {
          console.error('navigationStorageSync: invokeMethodAsync failed', err);
        });
      }
    } catch (err) {
      console.error('navigationStorageSync.onStorage error', err);
    }
  }

  window.navigationStorageSync = {
    registerDotNet: function (dotNetObject, key) {
      try {
        dotNetRef = dotNetObject;
        keyToWatch = key;
        window.addEventListener('storage', onStorage);
        // return true for success
        return true;
      } catch (err) {
        console.error('navigationStorageSync.registerDotNet error', err);
        return false;
      }
    },
    unregisterDotNet: function () {
      try {
        window.removeEventListener('storage', onStorage);
      } catch (err) {
        console.error('navigationStorageSync.unregisterDotNet error', err);
      }
      dotNetRef = null;
      keyToWatch = null;
    }
  };
})();
