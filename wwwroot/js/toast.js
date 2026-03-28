/**
 * toast.js — Sistema de notificaciones tipo toast
 *
 * API pública:
 *   mostrarToast(mensaje, tipo)
 *     tipo: 'exito' | 'error' | 'info' | 'atencion'
 *
 *   cerrarToast(btn)  — llamado desde el botón × del toast
 */
(function () {
    var AUTO_DISMISS_MS = 5000;
    var ICONOS = { exito: '✓', error: '✕', info: 'i', atencion: '!' };

    function mostrarToast(mensaje, tipo) {
        var container = document.getElementById('toast-container');
        if (!container) return;
        tipo = tipo || 'info';

        var toast = document.createElement('div');
        toast.className = 'toast toast-' + tipo;
        toast.innerHTML =
            '<span class="toast-icon">' + (ICONOS[tipo] || 'i') + '</span>' +
            '<span class="toast-mensaje">' + mensaje + '</span>' +
            '<button class="toast-close" onclick="cerrarToast(this)" aria-label="Cerrar">\u00D7</button>' +
            '<div class="toast-progress"></div>';

        container.appendChild(toast);

        var timer = setTimeout(function () { _dismiss(toast); }, AUTO_DISMISS_MS);
        toast.dataset.timerId = timer;
    }

    function cerrarToast(btn) {
        var toast = btn.closest('.toast');
        clearTimeout(parseInt(toast.dataset.timerId, 10));
        _dismiss(toast);
    }

    function _dismiss(toast) {
        if (!toast || toast.classList.contains('toast-saliendo')) return;
        toast.classList.add('toast-saliendo');
        setTimeout(function () { if (toast.parentNode) toast.remove(); }, 250);
    }

    window.mostrarToast = mostrarToast;
    window.cerrarToast  = cerrarToast;
})();
