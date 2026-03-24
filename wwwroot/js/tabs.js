/**
 * tabs.js — Componente de pestañas unificado
 *
 * initTabs(tabs, onSwitch?)
 *   tabs     — array de ids de pestaña (string[])
 *   onSwitch — callback opcional invocado con el id activo tras cada cambio
 *
 * Convención de nombres en el HTML:
 *   Botón  → id="tab-{id}"
 *   Panel  → id="panel-{id}"
 *   Activo → clase CSS "active"
 *
 * Expone window.switchTab(id) para uso en atributos onclick.
 */
function initTabs(tabs, onSwitch) {
    window.switchTab = function (id) {
        tabs.forEach(function (t) {
            var panel = document.getElementById('panel-' + t);
            var btn   = document.getElementById('tab-' + t);
            if (panel) panel.style.display = (t === id) ? 'block' : 'none';
            if (btn)   btn.classList.toggle('active', t === id);
        });
        if (typeof onSwitch === 'function') onSwitch(id);
    };
}
