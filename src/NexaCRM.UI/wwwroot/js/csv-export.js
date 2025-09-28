(function(){
    if (window.downloadCsv && window.downloadCsv._nexacrm_init) return;

    const _downloadCsv = (filename, content) => {
        const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const objectUrl = URL.createObjectURL(blob);
        link.href = objectUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        if (link && link.parentNode) {
            link.parentNode.removeChild(link);
        }
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
