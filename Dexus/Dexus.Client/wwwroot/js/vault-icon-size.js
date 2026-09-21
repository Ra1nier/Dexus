const storageKey = "dexus.vault.iconSize";

export function initialize(control) {
    const shell = control.closest(".vault-manager-shell");
    const slider = control.querySelector("input");
    const output = control.querySelector("output");
    const reset = control.querySelector("button");
    if (!shell) return;

    function apply(value, persist = false) {
        const number = Number(value);
        const percent = Number.isFinite(number)
            ? Math.min(150, Math.max(75, Math.round(number / 5) * 5))
            : 100;
        slider.value = String(percent);
        slider.setAttribute("aria-valuetext", `${percent}% of responsive size`);
        output.value = `${percent}%`;
        shell.style.setProperty("--vault-icon-scale", String(percent / 100));
        if (persist) {
            try { localStorage.setItem(storageKey, String(percent)); }
            catch { /* Sizing still works when browser storage is unavailable. */ }
        }
    }

    let saved = "100";
    try { saved = localStorage.getItem(storageKey) || "100"; }
    catch { /* Use the responsive default when storage is unavailable. */ }
    apply(saved);

    const onInput = () => apply(slider.value, true);
    const onReset = () => apply(100, true);
    slider.addEventListener("input", onInput);
    reset.addEventListener("click", onReset);

    const observer = new MutationObserver(() => {
        if (!control.isConnected) {
            slider.removeEventListener("input", onInput);
            reset.removeEventListener("click", onReset);
            observer.disconnect();
        }
    });
    observer.observe(document.body, { childList: true, subtree: true });
}
