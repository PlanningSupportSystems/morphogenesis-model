mergeInto(LibraryManager.library, {
    DownloadFramesZip: function (framesJsonPtr, zipNamePtr) {
        var framesJson = UTF8ToString(framesJsonPtr);
        var zipName = UTF8ToString(zipNamePtr);

        function run() {
            var frames = JSON.parse(framesJson);
            var zip = new JSZip();

            for (var i = 0; i < frames.length; i++) {
                var binary = atob(frames[i].data);
                var bytes = new Uint8Array(binary.length);

                for (var j = 0; j < binary.length; j++) {
                    bytes[j] = binary.charCodeAt(j);
                }

                zip.file(frames[i].name, bytes);
            }

            zip.generateAsync({ type: "blob" }).then(function (blob) {
                var url = URL.createObjectURL(blob);
                var link = document.createElement("a");

                link.href = url;
                link.download = zipName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

                URL.revokeObjectURL(url);
            });
        }

        if (typeof JSZip !== "undefined") {
            run();
            return;
        }

        var script = document.createElement("script");
        script.src = "https://cdnjs.cloudflare.com/ajax/libs/jszip/3.10.1/jszip.min.js";
        script.onload = run;
        script.onerror = function () {
            console.error("Nao foi possivel carregar JSZip.");
        };
        document.head.appendChild(script);
    }
});

mergeInto(LibraryManager.library, {
    DownloadFramesGif: function (framesJsonPtr, gifNamePtr, delayMs) {
        var framesJson = UTF8ToString(framesJsonPtr);
        var gifName = UTF8ToString(gifNamePtr);

        function carregarScript(src, globalName, callback) {
            if (window[globalName]) {
                callback();
                return;
            }

            var script = document.createElement("script");
            script.src = src;
            script.onload = callback;
            script.onerror = function () {
                console.error("Nao foi possivel carregar " + src);
            };
            document.head.appendChild(script);
        }

        function carregarImagem(dataUrl) {
            return new Promise(function (resolve, reject) {
                var img = new Image();
                img.onload = function () { resolve(img); };
                img.onerror = reject;
                img.src = dataUrl;
            });
        }

        function baixarBlob(blob, filename) {
            var url = URL.createObjectURL(blob);
            var link = document.createElement("a");

            link.href = url;
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            URL.revokeObjectURL(url);
        }

        function criarGif() {
            var frames = JSON.parse(framesJson);

            if (!frames || frames.length === 0) {
                console.warn("DownloadFramesGif: nenhuma imagem recebida.");
                return;
            }

            var promessas = frames.map(function (frame) {
                return carregarImagem("data:image/png;base64," + frame.data);
            });

            Promise.all(promessas).then(function (imagens) {
                var gif = new GIF({
                    workers: 2,
                    quality: 10,
                    workerScript: "gif.worker.js"
                });

                for (var i = 0; i < imagens.length; i++) {
                    var img = imagens[i];
                    var canvas = document.createElement("canvas");
                    var ctx = canvas.getContext("2d");

                    canvas.width = img.width;
                    canvas.height = img.height;
                    ctx.drawImage(img, 0, 0);

                    gif.addFrame(canvas, {
                        copy: true,
                        delay: delayMs
                    });
                }

                gif.on("finished", function (blob) {
                    baixarBlob(blob, gifName);
                });

                gif.render();
            }).catch(function (erro) {
                console.error("DownloadFramesGif: erro ao carregar frames.", erro);
            });
        }

        carregarScript(
            "gif.js",
            "GIF",
            criarGif
        );
    },

    DownloadFramesZipWithGif: function (framesJsonPtr, zipNamePtr, gifNamePtr, delayMs) {
    var framesJson = UTF8ToString(framesJsonPtr);
    var zipName = UTF8ToString(zipNamePtr);
    var gifName = UTF8ToString(gifNamePtr);

    function carregarScript(src, globalName) {
        return new Promise(function (resolve, reject) {
            if (window[globalName]) {
                resolve();
                return;
            }

            var script = document.createElement("script");
            script.src = src;
            script.onload = resolve;
            script.onerror = function () {
                reject("Nao foi possivel carregar " + src);
            };
            document.head.appendChild(script);
        });
    }

    function carregarImagem(dataUrl) {
        return new Promise(function (resolve, reject) {
            var img = new Image();
            img.onload = function () { resolve(img); };
            img.onerror = reject;
            img.src = dataUrl;
        });
    }

    function baixarBlob(blob, filename) {
        var url = URL.createObjectURL(blob);
        var link = document.createElement("a");

        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        URL.revokeObjectURL(url);
    }

    function base64ParaBytes(base64) {
        var binary = atob(base64);
        var bytes = new Uint8Array(binary.length);

        for (var i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }

        return bytes;
    }

    function criarGifBlob(frames, delay) {
        return Promise.all(frames.map(function (frame) {
            return carregarImagem("data:image/png;base64," + frame.data);
        })).then(function (imagens) {
            return new Promise(function (resolve, reject) {
                var gif = new GIF({
                    workers: 2,
                    quality: 10,
                    workerScript: "gif.worker.js"
                });

                for (var i = 0; i < imagens.length; i++) {
                    var img = imagens[i];
                    var canvas = document.createElement("canvas");
                    var ctx = canvas.getContext("2d");

                    canvas.width = img.width;
                    canvas.height = img.height;
                    ctx.drawImage(img, 0, 0);

                    gif.addFrame(canvas, {
                        copy: true,
                        delay: delay
                    });
                }

                gif.on("finished", function (blob) {
                    resolve(blob);
                });

                gif.on("abort", function () {
                    reject("GIF abortado.");
                });

                gif.render();
            });
        });
    }

    Promise.all([
        carregarScript("https://cdnjs.cloudflare.com/ajax/libs/jszip/3.10.1/jszip.min.js", "JSZip"),
        carregarScript("gif.js", "GIF")
    ]).then(function () {
        var frames = JSON.parse(framesJson);

        if (!frames || frames.length === 0) {
            console.warn("DownloadFramesZipWithGif: nenhuma imagem recebida.");
            return;
        }

        var zip = new JSZip();

        for (var i = 0; i < frames.length; i++) {
            zip.file(frames[i].name, base64ParaBytes(frames[i].data));
        }

        return criarGifBlob(frames, delayMs).then(function (gifBlob) {
            zip.file(gifName, gifBlob);

            return zip.generateAsync({ type: "blob" });
        }).then(function (zipBlob) {
            baixarBlob(zipBlob, zipName);
                if (typeof SendMessage !== "undefined") {
        SendMessage("PaineisBotoes", "FimDownload");
    }
        });
    }).catch(function (erro) {
        console.error("DownloadFramesZipWithGif: erro.", erro);
            
        if (typeof SendMessage !== "undefined") {
            SendMessage("PaineisBotoes", "FimDownload");
            }
    });
}
});


