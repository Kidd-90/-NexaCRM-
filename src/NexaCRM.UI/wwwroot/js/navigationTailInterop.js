const breakpoints = [
  { name: 'mobile', query: '(max-width: 767.98px)' },
  { name: 'tablet', query: '(min-width: 768px) and (max-width: 1023.98px)' },
  { name: 'desktop', query: '(min-width: 1024px) and (max-width: 1439.98px)' },
  { name: 'widescreen', query: '(min-width: 1440px)' }
];

let mediaEntries = [];
let dotNetRef = null;
let currentCategory = 'desktop';

function cleanup() {
  mediaEntries.forEach(entry => {
    if (!entry || !entry.mq) {
      return;
    }

    if (typeof entry.mq.removeEventListener === 'function') {
      entry.mq.removeEventListener('change', entry.handler);
    } else if (typeof entry.mq.removeListener === 'function') {
      entry.mq.removeListener(entry.handler);
    }
  });

  mediaEntries = [];
}

function evaluateCategory() {
  if (mediaEntries.length === 0) {
    if (typeof window !== 'undefined' && typeof window.innerWidth === 'number') {
      if (window.innerWidth < 768) {
        return 'mobile';
      }

      if (window.innerWidth < 1024) {
        return 'tablet';
      }

      if (window.innerWidth >= 1440) {
        return 'widescreen';
      }
    }

    return 'desktop';
  }

  for (let i = mediaEntries.length - 1; i >= 0; i -= 1) {
    const entry = mediaEntries[i];
    if (entry && entry.mq && entry.mq.matches) {
      return entry.bp.name;
    }
  }

  return 'desktop';
}

function notifyViewportChange() {
  const next = evaluateCategory();
  if (next === currentCategory) {
    return;
  }

  currentCategory = next;

  if (dotNetRef && typeof dotNetRef.invokeMethodAsync === 'function') {
    dotNetRef.invokeMethodAsync('OnViewportCategoryChanged', currentCategory).catch(err => {
      console.error('navigationTailInterop: failed to notify category change', err);
    });
  }
}

export function registerViewportObserver(reference) {
  dotNetRef = reference || null;
  cleanup();

  if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') {
    currentCategory = evaluateCategory();
    return currentCategory;
  }

  mediaEntries = breakpoints.map(bp => {
    const mq = window.matchMedia(bp.query);
    const handler = () => notifyViewportChange();

    if (typeof mq.addEventListener === 'function') {
      mq.addEventListener('change', handler);
    } else if (typeof mq.addListener === 'function') {
      mq.addListener(handler);
    }

    return { bp, mq, handler };
  });

  currentCategory = evaluateCategory();
  return currentCategory;
}

export function dispose() {
  cleanup();
  dotNetRef = null;
  currentCategory = 'desktop';
}

export const navigationTail = {
  registerViewportObserver,
  dispose
};
