// Renders the user's city as an isometric SVG from server-provided JSON
// (see Transactions/Index). Each category is a building whose height reflects
// its level; purchased decorations sit on empty lots and characters walk the
// streets. Pure SVG + CSS, no dependencies.
(function () {
    const host = document.getElementById('city');
    const dataEl = document.getElementById('cityData');
    if (!host || !dataEl) return;

    const data = JSON.parse(dataEl.textContent);
    if (!data) return;
    const NS = 'http://www.w3.org/2000/svg';
    const N = data.grid;
    const TW = 92, TH = 46;          // tile width / height (2:1 isometric)
    const FLOOR = 14, BASE = 16;     // building height per level / ground floor
    const PAD_TOP = 120;             // room above the back row for tall buildings
    const width = N * TW + 40;
    const height = PAD_TOP + N * TH + 40;
    const ox = width / 2;

    // Time-of-day sky, based on the viewer's local clock.
    const hour = new Date().getHours();
    host.classList.add(hour >= 7 && hour < 17 ? 'sky-day' : hour >= 17 && hour < 20 ? 'sky-dusk' : 'sky-night');

    const svg = el('svg', { viewBox: `0 0 ${width} ${height}`, class: 'city-svg', role: 'img', 'aria-label': data.labels.aria });

    function el(name, attrs, text) {
        const node = document.createElementNS(NS, name);
        for (const [k, v] of Object.entries(attrs || {})) node.setAttribute(k, v);
        if (text != null) node.textContent = text;
        return node;
    }

    function centre(slot) {
        const r = Math.floor(slot / N), c = slot % N;
        return { r, c, x: ox + (c - r) * TW / 2, y: PAD_TOP + (c + r) * TH / 2 + TH / 2 };
    }

    function rhombus(x, y, s) {
        const w = TW / 2 * s, d = TH / 2 * s;
        return `${x},${y - d} ${x + w},${y} ${x},${y + d} ${x - w},${y}`;
    }

    function shade(hex, pct) {
        const n = parseInt(hex.slice(1), 16);
        const f = c => Math.max(0, Math.min(255, Math.round(c + c * pct)));
        return `rgb(${f(n >> 16)},${f((n >> 8) & 255)},${f(n & 255)})`;
    }

    // ---- ground ------------------------------------------------------------
    const occupied = new Set([...data.buildings.map(b => b.slot), ...data.decorations.map(d => d.slot)]);
    const ground = el('g', { class: 'city-ground' });
    for (let slot = 0; slot < N * N; slot++) {
        const { r, c, x, y } = centre(slot);
        const fill = occupied.has(slot) ? '#dfe6ee' : ((r + c) % 2 ? '#9fdc8f' : '#93d283');
        ground.appendChild(el('polygon', { points: rhombus(x, y, 1), fill, stroke: 'rgba(0,0,0,.06)' }));
    }
    svg.appendChild(ground);

    // ---- things that stand on tiles, back-to-front (painter's algorithm) -----
    const items = [
        ...data.buildings.map(b => ({ kind: 'building', ...b })),
        ...data.decorations.map(d => ({ kind: 'decoration', ...d }))
    ].sort((a, b) => {
        const pa = centre(a.slot), pb = centre(b.slot);
        return (pa.r + pa.c) - (pb.r + pb.c);
    });

    // Levels from the previous visit, to celebrate buildings that grew.
    let seen = {};
    if (!data.preview) try { seen = JSON.parse(localStorage.getItem('mp.cityLevels') || '{}'); } catch { /* storage blocked */ }
    const now = {};

    for (const item of items) {
        const { x, y } = centre(item.slot);
        if (item.kind === 'decoration') {
            svg.appendChild(el('text', { x, y: y + 2, class: 'city-emoji city-deco', 'text-anchor': 'middle', 'dominant-baseline': 'middle' }, item.emoji));
            continue;
        }

        const b = item;
        now[b.category] = b.level;
        const grew = seen[b.category] !== undefined && b.level > seen[b.category];
        const h = BASE + b.level * FLOOR;
        const s = 0.62, w = TW / 2 * s, d = TH / 2 * s;
        const g = el('g', { class: 'city-building' + (grew ? ' level-up' : ''), tabindex: '0' });

        const tip = b.next
            ? `${b.name} · ${data.labels.level} ${b.level} · ${b.count} ${data.labels.entries} · ${data.labels.nextAt} ${b.next}`
            : `${b.name} · ${data.labels.level} ${b.level} · ${b.count} ${data.labels.entries} · ${data.labels.maxLevel}`;
        g.appendChild(el('title', {}, tip));

        // Left face, right face, roof.
        g.appendChild(el('polygon', { points: `${x - w},${y} ${x},${y + d} ${x},${y + d - h} ${x - w},${y - h}`, fill: shade(b.color, -0.12) }));
        g.appendChild(el('polygon', { points: `${x},${y + d} ${x + w},${y} ${x + w},${y - h} ${x},${y + d - h}`, fill: shade(b.color, -0.32) }));
        g.appendChild(el('polygon', { points: rhombus(x, y - h, s), fill: shade(b.color, 0.12) }));

        // Floor lines make the level readable at a glance.
        for (let i = 1; i < b.level; i++) {
            const lift = BASE + i * FLOOR; // height of the i-th floor boundary
            g.appendChild(el('polyline', { points: `${x - w},${y - lift} ${x},${y + d - lift} ${x + w},${y - lift}`, class: 'city-floor' }));
        }
        // Door.
        g.appendChild(el('polygon', { points: `${x + w * 0.35},${y + d * 0.65} ${x + w * 0.6},${y + d * 0.4} ${x + w * 0.6},${y + d * 0.4 - 11} ${x + w * 0.35},${y + d * 0.65 - 11}`, fill: 'rgba(0,0,0,.25)' }));

        g.appendChild(el('text', { x, y: y - h - d - 4, class: 'city-emoji', 'text-anchor': 'middle' }, b.emoji));
        g.appendChild(el('circle', { cx: x + w - 2, cy: y - h - 2, r: 8, class: 'city-badge' }));
        g.appendChild(el('text', { x: x + w - 2, y: y - h + 2, class: 'city-badge-text', 'text-anchor': 'middle' }, b.level));
        svg.appendChild(g);
    }

    // The landing-page preview is sample data; never let it overwrite the user's own history.
    if (!data.preview) {
        try { localStorage.setItem('mp.cityLevels', JSON.stringify(now)); } catch { /* storage blocked */ }
    }

    // ---- walkers -------------------------------------------------------------
    // Characters loop around the town along the outer ring of tiles.
    const ring = [0, N - 1, N * N - 1, N * (N - 1)].map(centre);
    const path = `M${ring[0].x},${ring[0].y} L${ring[1].x},${ring[1].y} L${ring[2].x},${ring[2].y} L${ring[3].x},${ring[3].y} Z`;
    data.characters.forEach((ch, i) => {
        const g = el('g', { class: 'city-walker' });
        g.appendChild(el('text', { x: 0, y: 0, class: 'city-emoji', 'text-anchor': 'middle' }, ch.emoji));
        if (data.outfit) g.appendChild(el('text', { x: 0, y: -18, class: 'city-outfit', 'text-anchor': 'middle' }, data.outfit));
        const dur = 28 + i * 6;
        g.appendChild(el('animateMotion', { dur: `${dur}s`, repeatCount: 'indefinite', path, begin: `-${i * (dur / Math.max(1, data.characters.length))}s` }));
        svg.appendChild(g);
    });

    // ---- empty state ---------------------------------------------------------
    if (!data.buildings.length) {
        const { x, y } = centre(Math.floor(N * N / 2) - Math.floor(N / 2));
        svg.appendChild(el('text', { x, y: y - 30, class: 'city-emoji city-empty-emoji', 'text-anchor': 'middle' }, '🚧'));
        svg.appendChild(el('text', { x, y: y + 18, class: 'city-empty-text', 'text-anchor': 'middle' }, data.labels.empty));
    }

    host.appendChild(svg);
})();
