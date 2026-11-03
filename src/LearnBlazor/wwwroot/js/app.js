window.learnBlazor = {
    download(fileName, text) {
        const url = URL.createObjectURL(new Blob([text], { type: "text/markdown" }));
        const a = document.createElement("a");
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
    }
};
