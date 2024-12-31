mergeInto(LibraryManager.library, {
    SaveAllFiles: function (filenamesPtr, contentsPtr, lengthsPtr, count) {
        var filenames = [];
        var contents = [];

        for (var i = 0; i < count; i++) {
            var filename = UTF8ToString(getValue(filenamesPtr + i * 4, '*'));
            filenames.push(filename);

            var contentLength = getValue(lengthsPtr + i * 4, 'i32');
            var content = new Uint8Array(Module.HEAPU8.buffer, getValue(contentsPtr + i * 4, '*'), contentLength);
            contents.push(content);
        }

        var zip = new JSZip();

        for (var i = 0; i < filenames.length; i++) {
            zip.file(filenames[i], contents[i]);
        }

        zip.generateAsync({ type: "blob" }).then(function (blob) {
            var link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.download = 'screenshots.zip';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        });
    }
});
