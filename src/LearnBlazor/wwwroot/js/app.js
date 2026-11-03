// Code textboxes (.code-input): Tab inserts 4 spaces instead of moving focus.
// Press Esc first to let the next Tab move focus as usual (keyboard users must be able to leave the box).
(() => {
    let escaped = false;
    document.addEventListener("keydown", (e) => {
        const box = e.target;
        if (!(box instanceof HTMLTextAreaElement) || !box.classList.contains("code-input")) return;
        if (e.key === "Escape") { escaped = true; return; }
        if (e.key !== "Tab" || e.shiftKey || e.ctrlKey || e.altKey || e.metaKey || escaped) {
            escaped = false;
            return;
        }
        e.preventDefault();
        // execCommand keeps the browser's undo history; fall back to setRangeText where it is missing.
        if (!document.execCommand("insertText", false, "    ")) {
            box.setRangeText("    ", box.selectionStart, box.selectionEnd, "end");
            box.dispatchEvent(new Event("input", { bubbles: true }));
        }
    });
})();
