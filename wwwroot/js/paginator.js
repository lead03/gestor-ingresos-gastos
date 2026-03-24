/**
 * paginator.js — Paginado cliente para tablas y listas
 *
 * initPaginator(containerId, controlsId, pageSize?, itemSelector?)
 *   containerId  — id del contenedor (<tbody>, <ul>, etc.)
 *   controlsId   — id del <div> donde se renderizan los controles
 *   pageSize     — ítems por página (default 10)
 *   itemSelector — selector CSS para los ítems (default 'tr', usar 'li' para listas)
 *
 * initPaginatorGrouped(tbodyId, controlsId, groupClass, pageSize?)
 *   Igual que initPaginator pero omite filas con groupClass del conteo y las
 *   muestra u oculta según si alguno de sus hijos es visible en la página actual.
 */

/* ── helpers compartidos ─────────────────────────────────────────────── */

function _pgNums(cur, tot) {
    var set = {};
    set[1] = true; set[tot] = true;
    for (var p = Math.max(1, cur - 1); p <= Math.min(tot, cur + 1); p++) set[p] = true;
    return Object.keys(set).map(Number).sort(function (a, b) { return a - b; });
}

function _pgMakeBtn(label, enabled, onClick) {
    var b = document.createElement('button');
    b.type      = 'button';
    b.className = 'paginator-btn' + (enabled ? '' : ' paginator-disabled');
    b.textContent = label;
    if (enabled) b.addEventListener('click', onClick);
    return b;
}

function _pgMakePageBtn(n, isActive, onClick) {
    var b = document.createElement('button');
    b.type      = 'button';
    b.className = 'paginator-btn' + (isActive ? ' paginator-active' : '');
    b.textContent = n;
    b.addEventListener('click', onClick);
    return b;
}

function _pgRenderControls(controls, page, total, from, to, itemCount, onPrev, onNext, onPage) {
    controls.innerHTML = '';
    var wrap = document.createElement('div');
    wrap.className = 'paginator';

    var info = document.createElement('span');
    info.className   = 'paginator-info';
    info.textContent = from + '–' + to + ' de ' + itemCount;
    wrap.appendChild(info);

    wrap.appendChild(_pgMakeBtn('‹ Ant.', page > 1, onPrev));

    var nums = _pgNums(page, total);
    var prev = null;
    nums.forEach(function (n) {
        if (prev !== null && n - prev > 1) {
            var sep = document.createElement('span');
            sep.className   = 'paginator-sep';
            sep.textContent = '…';
            wrap.appendChild(sep);
        }
        wrap.appendChild(_pgMakePageBtn(n, n === page, onPage(n)));
        prev = n;
    });

    wrap.appendChild(_pgMakeBtn('Sig. ›', page < total, onNext));
    controls.appendChild(wrap);
}

/* ── initPaginator ───────────────────────────────────────────────────── */

function initPaginator(containerId, controlsId, pageSize, itemSelector) {
    pageSize     = pageSize     || 10;
    itemSelector = itemSelector || 'tr';

    var container = document.getElementById(containerId);
    var controls  = document.getElementById(controlsId);
    if (!container || !controls) return;

    var items = Array.from(container.querySelectorAll(itemSelector));
    if (items.length <= pageSize) return;

    var page  = 1;
    var total = Math.ceil(items.length / pageSize);

    function show() {
        var from = (page - 1) * pageSize;
        var to   = from + pageSize;
        items.forEach(function (r, i) {
            r.style.display = (i >= from && i < to) ? '' : 'none';
        });
        var dispFrom = (page - 1) * pageSize + 1;
        var dispTo   = Math.min(page * pageSize, items.length);
        _pgRenderControls(
            controls, page, total, dispFrom, dispTo, items.length,
            function () { page--; show(); },
            function () { page++; show(); },
            function (n) { return function () { page = n; show(); }; }
        );
    }

    show();
}

/* ── initPaginatorGrouped ────────────────────────────────────────────── */

function initPaginatorGrouped(tbodyId, controlsId, groupClass, pageSize) {
    pageSize = pageSize || 10;

    var tbody    = document.getElementById(tbodyId);
    var controls = document.getElementById(controlsId);
    if (!tbody || !controls) return;

    var allRows  = Array.from(tbody.querySelectorAll('tr'));
    var dataRows = allRows.filter(function (r) { return !r.classList.contains(groupClass); });

    if (dataRows.length <= pageSize) return;

    var page  = 1;
    var total = Math.ceil(dataRows.length / pageSize);

    function show() {
        var from = (page - 1) * pageSize;
        var to   = from + pageSize;

        // Mostrar / ocultar filas de datos
        dataRows.forEach(function (r, i) {
            r.style.display = (i >= from && i < to) ? '' : 'none';
        });

        // Mostrar / ocultar cabeceras de grupo según si tienen datos visibles
        allRows
            .filter(function (r) { return r.classList.contains(groupClass); })
            .forEach(function (gr) {
                var idx        = allRows.indexOf(gr);
                var hasVisible = false;
                for (var i = idx + 1; i < allRows.length; i++) {
                    if (allRows[i].classList.contains(groupClass)) break;
                    if (allRows[i].style.display !== 'none') { hasVisible = true; break; }
                }
                gr.style.display = hasVisible ? '' : 'none';
            });

        var dispFrom = (page - 1) * pageSize + 1;
        var dispTo   = Math.min(page * pageSize, dataRows.length);
        _pgRenderControls(
            controls, page, total, dispFrom, dispTo, dataRows.length,
            function () { page--; show(); },
            function () { page++; show(); },
            function (n) { return function () { page = n; show(); }; }
        );
    }

    show();
}
