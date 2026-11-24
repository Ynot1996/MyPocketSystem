// Amount entry helpers for the add-transaction form: per-currency symbol, the
// ± stepper (uses each currency's natural step) and quick-pick presets.
// Exposes window.MyPocketForm so capture.js can switch currency after a scan.
(function () {
    const select = document.getElementById('Currency');
    const amount = document.getElementById('Amount');
    if (!select || !amount) return;

    const symbolEl = document.getElementById('amountSymbol');
    const presets = document.getElementById('amountPresets');

    function meta() {
        const opt = select.options[select.selectedIndex];
        return {
            symbol: opt.dataset.symbol,
            decimals: parseInt(opt.dataset.decimals, 10),
            step: parseFloat(opt.dataset.step),
            presets: (opt.dataset.presets || '').split(',').filter(Boolean).map(Number)
        };
    }

    function renderPresets() {
        const m = meta();
        presets.innerHTML = '';
        m.presets.forEach(v => {
            const b = document.createElement('button');
            b.type = 'button';
            b.className = 'btn btn-sm btn-outline-primary preset-btn';
            b.textContent = m.symbol + v.toLocaleString(undefined, { minimumFractionDigits: m.decimals, maximumFractionDigits: m.decimals });
            b.addEventListener('click', () => { amount.value = v.toFixed(m.decimals); amount.dispatchEvent(new Event('input')); });
            presets.appendChild(b);
        });
    }

    function applyCurrency() {
        const m = meta();
        symbolEl.textContent = m.symbol;
        // The input keeps step="any" so any decimal (e.g. 3.67) is accepted;
        // the currency step only drives the ± buttons.
        const current = parseFloat(amount.value);
        if (!isNaN(current)) amount.value = current.toFixed(m.decimals);
        renderPresets();
    }

    function nudge(direction) {
        const m = meta();
        const current = parseFloat(amount.value) || 0;
        amount.value = Math.max(0, current + direction * m.step).toFixed(m.decimals);
    }

    select.addEventListener('change', applyCurrency);
    document.getElementById('amountUp')?.addEventListener('click', () => nudge(1));
    document.getElementById('amountDown')?.addEventListener('click', () => nudge(-1));
    renderPresets();

    window.MyPocketForm = {
        setCurrency(code) {
            if (!code || select.value === code) return;
            if ([...select.options].some(o => o.value === code)) {
                select.value = code;
                applyCurrency();
            }
        }
    };
})();
