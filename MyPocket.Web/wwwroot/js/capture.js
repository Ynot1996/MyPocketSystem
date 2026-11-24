// Auto-capture: receipt photo, voice and one-line text all produce a draft that
// pre-fills the add-transaction form. The user always reviews and saves it.
(function () {
    const bar = document.querySelector('.capture-bar');
    const form = document.getElementById('createTransactionForm');
    if (!bar || !form) return;

    const statusEl = document.getElementById('captureStatus');
    const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenInput ? tokenInput.value : '';
    const msg = bar.dataset;

    function setStatus(text, kind) {
        statusEl.textContent = text || '';
        statusEl.className = 'capture-status small mt-2' + (kind ? ' is-' + kind : '');
    }

    function setBusy(busy) {
        bar.classList.toggle('is-busy', busy);
        bar.querySelectorAll('button, input').forEach(el => { el.disabled = busy; });
    }

    function fill(draft) {
        if (draft.currency) window.MyPocketForm?.setCurrency(draft.currency);
        if (draft.amount != null) document.getElementById('Amount').value = draft.amount;
        if (draft.categoryId) document.getElementById('CategoryId').value = draft.categoryId;
        if (draft.description) document.getElementById('Description').value = draft.description;
        if (draft.date) {
            // Keep the current time of day; receipts rarely show a reliable time.
            const time = new Date().toTimeString().slice(0, 5);
            document.getElementById('TransactionDate').value = draft.date + 'T' + time;
        }
        document.getElementById('captureSource').value = draft.source || '';

        form.classList.remove('just-filled');
        void form.offsetWidth; // restart the highlight animation
        form.classList.add('just-filled');
        setStatus(draft.usedAi ? msg.msgFilled : msg.msgFilledOffline, 'ok');
        form.querySelector('[type=submit]').focus();
    }

    async function handle(response) {
        if (response.status === 429) { setStatus(msg.msgLimit, 'warn'); return; }
        let body = {};
        try { body = await response.json(); } catch { /* non-JSON error */ }
        if (!response.ok) { setStatus(body.error || body.detail || msg.msgError, 'warn'); return; }
        fill(body);
    }

    // --- Receipt photo -------------------------------------------------------
    const receiptInput = document.getElementById('receiptInput');
    receiptInput?.addEventListener('change', async () => {
        const file = receiptInput.files && receiptInput.files[0];
        if (!file) return;
        const data = new FormData();
        data.append('image', file);
        setBusy(true);
        setStatus(msg.msgScanning, 'busy');
        try {
            await handle(await fetch(bar.dataset.receiptUrl, {
                method: 'POST',
                headers: { 'RequestVerificationToken': token },
                body: data
            }));
        } catch {
            setStatus(msg.msgError, 'warn');
        } finally {
            setBusy(false);
            receiptInput.value = '';
        }
    });

    // --- One-line text (also the target of voice input) ---------------------
    const quickText = document.getElementById('quickText');
    const quickTextBtn = document.getElementById('quickTextBtn');

    async function parseText(text) {
        if (!text || !text.trim()) return;
        setBusy(true);
        setStatus(msg.msgParsing, 'busy');
        try {
            await handle(await fetch(bar.dataset.textUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
                body: JSON.stringify({ text })
            }));
        } catch {
            setStatus(msg.msgError, 'warn');
        } finally {
            setBusy(false);
        }
    }

    quickTextBtn?.addEventListener('click', () => parseText(quickText.value));
    quickText?.addEventListener('keydown', e => {
        if (e.key === 'Enter') { e.preventDefault(); parseText(quickText.value); }
    });

    // --- Voice (Web Speech API; Chrome, Edge and Safari) ----------------------
    // Speech-to-text runs in the browser, so voice capture costs nothing extra;
    // only the resulting sentence is sent to the server.
    const Recognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    const voiceBtn = document.getElementById('voiceBtn');
    if (Recognition && voiceBtn) {
        voiceBtn.hidden = false;
        voiceBtn.addEventListener('click', () => {
            const rec = new Recognition();
            rec.lang = bar.dataset.speechLang || 'en-GB';
            rec.interimResults = false;
            rec.maxAlternatives = 1;
            voiceBtn.classList.add('is-listening');
            setStatus(msg.msgListening, 'busy');
            rec.onresult = e => {
                const transcript = e.results[0][0].transcript;
                quickText.value = transcript;
                parseText(transcript);
            };
            rec.onerror = () => setStatus(msg.msgError, 'warn');
            rec.onend = () => voiceBtn.classList.remove('is-listening');
            rec.start();
        });
    }
})();
