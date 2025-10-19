(function(){
    if (window.downloadCsv && window.downloadCsv._nexacrm_init) return;

    const detachNode = (node) => {
        if (!node) {
            return;
        }

        const parent = node.parentNode;
        if (parent && typeof parent.removeChild === 'function') {
            try {
                parent.removeChild(node);
                return;
            } catch (err) {
                console.warn('Failed to detach node via parent.removeChild', err);
            }
        }

        if (typeof node.remove === 'function') {
            try {
                node.remove();
            } catch (err) {
                console.warn('Failed to detach node via node.remove()', err);
            }
        }
    };

    const _downloadCsv = (filename, content) => {
        const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const objectUrl = URL.createObjectURL(blob);
        link.href = objectUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        detachNode(link);
        try {
            if (typeof URL !== 'undefined' && objectUrl) {
                URL.revokeObjectURL(objectUrl);
            }
        } catch (e) {
            console.warn('Failed to revoke object URL', e);
        }
    };

    _downloadCsv._nexacrm_init = true;
    window.downloadCsv = _downloadCsv;
})();
