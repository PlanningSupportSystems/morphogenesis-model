// JavaScript source code
mergeInto(LibraryManager.library, {
    DownloadImage: function (base64String, fileName) {
        var link = document.createElement('a');
        link.href = 'data:image/jpeg;base64,' + Module.UTF8ToString(base64String);
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
});
