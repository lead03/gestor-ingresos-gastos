/**
 * modal.js — Gestión de modales
 *
 * API pública (retrocompatible con todo el uso existente):
 *   openModal(id)  — abre el modal con ese id
 *   closeModal(id) — cierra el modal con ese id
 *   initModal(id)  — registra cierre al hacer clic en el fondo (backdrop)
 *
 * Funcionalidades agregadas sobre la versión anterior:
 *   - Tecla ESC cierra el modal más recientemente abierto
 *   - Bloquea el scroll del <body> mientras hay algún modal abierto
 *   - Fallback de ESC: si ningún modal fue abierto via openModal() pero hay
 *     uno con .open en el DOM, lo cierra igual (compatibilidad con páginas
 *     que abren modales con JS inline sin pasar por openModal)
 */

/** Pila de ids de modales abiertos en orden de apertura. */
var _modalStack = [];

/**
 * Abre el modal con el id dado.
 * @param {string} id
 */
function openModal(id) {
    var el = document.getElementById(id);
    if (!el) return;
    el.classList.add('open');
    if (_modalStack.indexOf(id) === -1) _modalStack.push(id);
    if (_modalStack.length === 1) document.body.style.overflow = 'hidden';
}

/**
 * Cierra el modal con el id dado.
 * @param {string} id
 */
function closeModal(id) {
    var el = document.getElementById(id);
    if (!el) return;
    el.classList.remove('open');
    var idx = _modalStack.lastIndexOf(id);
    if (idx !== -1) _modalStack.splice(idx, 1);
    if (_modalStack.length === 0) document.body.style.overflow = '';
}

/**
 * Registra el cierre al hacer clic en el fondo (backdrop).
 * Llamar una vez por modal en DOMContentLoaded.
 * @param {string} id
 */
function initModal(id) {
    var el = document.getElementById(id);
    if (!el) return;
    el.addEventListener('click', function (e) {
        if (e.target === this) closeModal(id);
    });
}

// ── Tecla ESC ────────────────────────────────────────────────────────────────
// Cierra el modal más reciente de la pila. Si la pila está vacía (modal abierto
// via JS inline sin pasar por openModal), busca el primero con .open en el DOM.
document.addEventListener('keydown', function (e) {
    if (e.key !== 'Escape') return;

    var topId = _modalStack[_modalStack.length - 1];
    if (topId) {
        closeModal(topId);
        return;
    }

    // Fallback: buscar cualquier .modal-bg.open en el DOM
    var openEl = document.querySelector('.modal-bg.open');
    if (openEl && openEl.id) closeModal(openEl.id);
});
