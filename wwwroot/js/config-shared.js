/**
 * config-shared.js — Helpers genéricos para entidades de Configuración.
 * Reutilizable por Banco, Billetera, RedTarjeta, etc.
 */

/**
 * Abre un _ModalConfirm para eliminar una entidad.
 * Ajusta data-tipo y contenido según si tiene dependencias o no.
 *
 * @param {string} modalId  - Id del modal (ej: "modal-eliminar-banco")
 * @param {string} entidad  - Nombre legible de la entidad (ej: "banco")
 * @param {string} nombre   - Nombre del registro a eliminar
 * @param {number} count    - Cantidad de dependencias
 * @param {string|number} id - Id del registro a eliminar
 * @param {string} [depLabel] - Texto de dependencias (ej: "cuentas asociadas"). Default: "elementos asociados"
 */
function abrirModalEliminar(modalId, entidad, nombre, count, id, depLabel) {
    depLabel = depLabel || 'elementos asociados';

    const modalEl       = document.getElementById(modalId);
    const titulo        = document.getElementById(modalId + '-titulo');
    const cuerpo        = document.getElementById(modalId + '-cuerpo');
    const footerInfo    = document.getElementById(modalId + '-footer-info');
    const footerConfirm = document.getElementById(modalId + '-footer-confirm');
    const idCampo       = document.getElementById(modalId + '-id-campo');

    if (count > 0) {
        // Bloqueado por dependencias → informativo
        modalEl.dataset.tipo   = 'informativo';
        const dep              = count === 1 ? `1 ${depLabel.replace(/s$/, '')}` : `${count} ${depLabel}`;
        titulo.textContent     = 'No se puede eliminar';
        cuerpo.textContent     = `"${nombre}" tiene ${dep}. Primero desasociá los registros vinculados y luego podés eliminarlo.`;
        footerInfo.style.display    = 'block';
        footerConfirm.style.display = 'none';
    } else {
        // Sin dependencias → peligro (acción irreversible)
        modalEl.dataset.tipo   = 'peligro';
        titulo.textContent     = `Eliminar ${entidad}`;
        cuerpo.textContent     = `¿Estás seguro de que querés eliminar "${nombre}"? Esta acción no se puede deshacer.`;
        footerInfo.style.display    = 'none';
        footerConfirm.style.display = 'flex';
        idCampo.value = id;
    }

    openModal(modalId);
}
