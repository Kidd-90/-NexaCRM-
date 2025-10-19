// Small helper to bridge DOM pull-to-refresh event with Blazor .NET
// Exposes: attach(dotNetRef), detach(), dispatchComplete()

let _dotNetRef = null;
let _listener = null;

export function attach(dotNetRef) {
  _dotNetRef = dotNetRef;
  _listener = (e) => {
    try {
      // When JS code dispatches the event it will be received here; call .NET
      if (_dotNetRef) {
        _dotNetRef.invokeMethodAsync('HandlePullToRefresh');
      }
    } catch (err) {
      console.error('Error invoking .NET HandlePullToRefresh', err);
    }
  };

  // Listen for the custom event dispatched by interactions.js
  window.addEventListener('nexacrm:pullToRefresh', _listener);
}

export function detach() {
  if (_listener) {
    window.removeEventListener('nexacrm:pullToRefresh', _listener);
    _listener = null;
  }
  _dotNetRef = null;
}

export function dispatchComplete() {
  try {
    const ev = new CustomEvent('nexacrm:pullToRefreshComplete');
    window.dispatchEvent(ev);
  } catch (err) {
    console.error('Error dispatching pullToRefreshComplete', err);
  }
}
