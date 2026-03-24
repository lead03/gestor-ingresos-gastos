/**
 * modal.js — Componente de modales genérico
 *
 * openModal(id)  — abre el modal con ese id
 * closeModal(id) — cierra el modal con ese id
 * initModal(id)  — registra el cierre al hacer clic en el fondo
 */
function openModal(id) {
    document.getElementById(id).classList.add('open');
}

function closeModal(id) {
    document.getElementById(id).classList.remove('open');
}

function initModal(id) {
    var el = document.getElementById(id);
    if (!el) return;
    el.addEventListener('click', function (e) {
        if (e.target === this) closeModal(id);
    });
}
