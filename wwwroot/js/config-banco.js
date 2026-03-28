/**
 * config-banco.js — Lógica de la entidad Banco en Configuración.
 * Depende de: modal.js, config-shared.js
 */

// ── Restricciones leídas desde data-val-* generados por asp-for ─
// Fuente de verdad: BancoVM annotations → ASP.NET genera atributos → JS los lee.
const _bancoRef = document.querySelector('#form-agregar-banco [name="FormBanco.Nombre"]');
const BANCO_MIN     = parseInt(_bancoRef?.getAttribute('data-val-minlength-min') ?? '2',  10);
const BANCO_MAX     = parseInt(_bancoRef?.getAttribute('data-val-maxlength-max') ?? '60', 10);
const BANCO_MSG_REQ = _bancoRef?.getAttribute('data-val-required')  ?? 'El nombre es obligatorio.';
const BANCO_MSG_MIN = _bancoRef?.getAttribute('data-val-minlength') ?? `Mínimo ${BANCO_MIN} caracteres.`;
const BANCO_MSG_MAX = _bancoRef?.getAttribute('data-val-maxlength') ?? `Máximo ${BANCO_MAX} caracteres.`;

// Propagar maxlength a todos los inputs de edición inline
document.querySelectorAll('.banco-nombre-input')
    .forEach(el => el.setAttribute('maxlength', BANCO_MAX));

// ── Inline edit ────────────────────────────────────────────────
function editarBancoInline(btn) {
    const tr = btn.closest('tr');
    tr.querySelector('.banco-nombre-display').style.display = 'none';
    tr.querySelector('.banco-nombre-input').style.display   = '';
    tr.querySelector('.banco-nombre-input').focus();
    tr.querySelector('.banco-acciones-display').style.display  = 'none';
    tr.querySelector('.banco-acciones-edicion').style.display  = 'flex';
}

function cancelarBancoInline(btn) {
    const tr = btn.closest('tr');
    tr.querySelector('.banco-nombre-input').value           = tr.dataset.nombre;
    tr.querySelector('.banco-nombre-error').style.display   = 'none';
    tr.querySelector('.banco-nombre-display').style.display = '';
    tr.querySelector('.banco-nombre-input').style.display   = 'none';
    tr.querySelector('.banco-acciones-display').style.display = 'flex';
    tr.querySelector('.banco-acciones-edicion').style.display = 'none';
}

function guardarBancoInline(btn) {
    const tr             = btn.closest('tr');
    const input          = tr.querySelector('.banco-nombre-input');
    const errorSpan      = tr.querySelector('.banco-nombre-error');
    const nombreNuevo    = input.value.trim();
    const nombreOriginal = tr.dataset.nombre;
    const cuentas        = parseInt(tr.dataset.cuentas, 10);
    const id             = tr.dataset.id;

    // Validación client-side — mensajes y límites desde BancoVM via data-val-*
    let error = '';
    if (!nombreNuevo)                          error = BANCO_MSG_REQ;
    else if (nombreNuevo.length < BANCO_MIN)   error = BANCO_MSG_MIN;
    else if (nombreNuevo.length > BANCO_MAX)   error = BANCO_MSG_MAX;

    if (error) {
        errorSpan.textContent    = error;
        errorSpan.style.display  = '';
        input.focus();
        return;
    }
    errorSpan.style.display = 'none';

    // Llenar form oculto
    document.getElementById('editar-banco-id-campo').value     = id;
    document.getElementById('editar-banco-nombre-campo').value = nombreNuevo;

    // Abrir modal de confirmación
    let cuerpo = `¿Renombrar "${nombreOriginal}" a "${nombreNuevo}"?`;
    if (cuentas > 0) {
        const dep = cuentas === 1 ? '1 cuenta vinculada' : `${cuentas} cuentas vinculadas`;
        cuerpo += ` Este banco tiene ${dep}.`;
    }
    document.getElementById('modal-confirmar-banco-titulo').textContent        = 'Confirmar cambio';
    document.getElementById('modal-confirmar-banco-cuerpo').textContent        = cuerpo;
    document.getElementById('modal-confirmar-banco-footer-info').style.display    = 'none';
    document.getElementById('modal-confirmar-banco-footer-confirm').style.display = 'flex';
    openModal('modal-confirmar-banco');
}

function submitEditarBanco() {
    document.getElementById('form-editar-banco-hidden').submit();
}

// ── Delete modal ───────────────────────────────────────────────
function confirmarEliminarBanco(btn) {
    const tr     = btn.closest('tr');
    const nombre = tr.dataset.nombre;
    const cuentas = parseInt(tr.dataset.cuentas, 10);
    const id     = tr.dataset.id;
    abrirModalEliminar('modal-eliminar-banco', 'banco', nombre, cuentas, id, 'cuentas asociadas');
}
