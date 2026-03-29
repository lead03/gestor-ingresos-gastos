/**
 * config-billetera.js — Lógica de Billetera en Configuración.
 * Depende de: modal.js, config-shared.js
 */

// ── Restricciones desde data-val-* generados por asp-for ────────
// Fuente de verdad: BilleteraVM annotations → ASP.NET → JS
const _billeteraRef    = document.querySelector('#form-agregar-billetera [name="FormBilletera.Nombre"]');
const BILLETERA_MIN     = parseInt(_billeteraRef?.getAttribute('data-val-minlength-min') ?? '2',  10);
const BILLETERA_MAX     = parseInt(_billeteraRef?.getAttribute('data-val-maxlength-max') ?? '60', 10);
const BILLETERA_MSG_REQ = _billeteraRef?.getAttribute('data-val-required')  ?? 'El nombre es obligatorio.';
const BILLETERA_MSG_MIN = _billeteraRef?.getAttribute('data-val-minlength') ?? `Mínimo ${BILLETERA_MIN} caracteres.`;
const BILLETERA_MSG_MAX = _billeteraRef?.getAttribute('data-val-maxlength') ?? `Máximo ${BILLETERA_MAX} caracteres.`;

// Propagar maxlength a inputs de edición inline
document.querySelectorAll('.billetera-nombre-input')
    .forEach(el => el.setAttribute('maxlength', BILLETERA_MAX));

// ── Inline edit ─────────────────────────────────────────────────
function editarBilleteraInline(btn) {
    const tr = btn.closest('tr');
    tr.querySelector('.billetera-nombre-display').style.display    = 'none';
    tr.querySelector('.billetera-nombre-input').style.display      = '';
    tr.querySelector('.billetera-nombre-input').focus();
    tr.querySelector('.billetera-acciones-display').style.display  = 'none';
    tr.querySelector('.billetera-acciones-edicion').style.display  = 'flex';
}

function cancelarBilleteraInline(btn) {
    const tr = btn.closest('tr');
    tr.querySelector('.billetera-nombre-input').value              = tr.dataset.nombre;
    tr.querySelector('.billetera-nombre-error').style.display      = 'none';
    tr.querySelector('.billetera-nombre-display').style.display    = '';
    tr.querySelector('.billetera-nombre-input').style.display      = 'none';
    tr.querySelector('.billetera-acciones-display').style.display  = 'flex';
    tr.querySelector('.billetera-acciones-edicion').style.display  = 'none';
}

function guardarBilleteraInline(btn) {
    const tr             = btn.closest('tr');
    const input          = tr.querySelector('.billetera-nombre-input');
    const errorSpan      = tr.querySelector('.billetera-nombre-error');
    const nombreNuevo    = input.value.trim();
    const nombreOriginal = tr.dataset.nombre;
    const dependientes   = parseInt(tr.dataset.dependientes, 10);
    const id             = tr.dataset.id;

    let error = '';
    if (!nombreNuevo)                                error = BILLETERA_MSG_REQ;
    else if (nombreNuevo.length < BILLETERA_MIN)     error = BILLETERA_MSG_MIN;
    else if (nombreNuevo.length > BILLETERA_MAX)     error = BILLETERA_MSG_MAX;

    if (error) {
        errorSpan.textContent   = error;
        errorSpan.style.display = '';
        input.focus();
        return;
    }
    errorSpan.style.display = 'none';

    document.getElementById('editar-billetera-id-campo').value     = id;
    document.getElementById('editar-billetera-nombre-campo').value = nombreNuevo;

    let cuerpo = `¿Renombrar "${nombreOriginal}" a "${nombreNuevo}"?`;
    if (dependientes > 0) {
        const dep = dependientes === 1
            ? `1 cuenta vinculada`
            : `${dependientes} cuentas vinculadas`;
        cuerpo += ` Tiene ${dep}.`;
    }
    document.getElementById('modal-confirmar-billetera-titulo').textContent        = 'Confirmar cambio';
    document.getElementById('modal-confirmar-billetera-cuerpo').textContent        = cuerpo;
    document.getElementById('modal-confirmar-billetera-footer-info').style.display    = 'none';
    document.getElementById('modal-confirmar-billetera-footer-confirm').style.display = 'flex';
    openModal('modal-confirmar-billetera');
}

function submitEditarBilletera() {
    document.getElementById('form-editar-billetera-hidden').submit();
}

// ── Delete modal ────────────────────────────────────────────────
function confirmarEliminarBilletera(btn) {
    const tr = btn.closest('tr');
    abrirModalEliminar(
        'modal-eliminar-billetera',
        'billetera',
        tr.dataset.nombre,
        parseInt(tr.dataset.dependientes, 10),
        tr.dataset.id,
        'cuentas asociadas'
    );
}
