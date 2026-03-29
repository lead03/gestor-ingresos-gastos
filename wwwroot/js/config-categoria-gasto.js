/**
 * config-categoria-gasto.js — Lógica de CategoriaGasto en Configuración.
 * Depende de: modal.js, config-shared.js
 */

// ── Restricciones desde data-val-* generados por asp-for ────────
// Fuente de verdad: CategoriaGastoVM annotations → ASP.NET → JS
const _categoriaGastoRef    = document.querySelector('#form-agregar-categoria-gasto [name="FormCategoriaGasto.Nombre"]');
const CATEGORIA_GASTO_MIN     = parseInt(_categoriaGastoRef?.getAttribute('data-val-minlength-min') ?? '2',  10);
const CATEGORIA_GASTO_MAX     = parseInt(_categoriaGastoRef?.getAttribute('data-val-maxlength-max') ?? '80', 10);
const CATEGORIA_GASTO_MSG_REQ = _categoriaGastoRef?.getAttribute('data-val-required')  ?? 'El nombre es obligatorio.';
const CATEGORIA_GASTO_MSG_MIN = _categoriaGastoRef?.getAttribute('data-val-minlength') ?? `Mínimo ${CATEGORIA_GASTO_MIN} caracteres.`;
const CATEGORIA_GASTO_MSG_MAX = _categoriaGastoRef?.getAttribute('data-val-maxlength') ?? `Máximo ${CATEGORIA_GASTO_MAX} caracteres.`;

// Propagar maxlength a inputs de edición inline
document.querySelectorAll('.categoria-gasto-nombre-input')
    .forEach(el => el.setAttribute('maxlength', CATEGORIA_GASTO_MAX));

// ── Inline edit ─────────────────────────────────────────────────
function editarCategoriaGastoInline(btn) {
    const tr = btn.closest('tr');
    tr.querySelector('.categoria-gasto-nombre-display').style.display    = 'none';
    tr.querySelector('.categoria-gasto-nombre-input').style.display      = '';
    tr.querySelector('.categoria-gasto-nombre-input').focus();
    tr.querySelector('.categoria-gasto-tipo-display').style.display      = 'none';
    tr.querySelector('.categoria-gasto-tipo-select').style.display       = '';
    tr.querySelector('.categoria-gasto-tipo-select').value               = tr.dataset.tipo;
    tr.querySelector('.categoria-gasto-acciones-display').style.display  = 'none';
    tr.querySelector('.categoria-gasto-acciones-edicion').style.display  = 'flex';
}

function cancelarCategoriaGastoInline(btn) {
    const tr = btn.closest('tr');
    tr.querySelector('.categoria-gasto-nombre-input').value              = tr.dataset.nombre;
    tr.querySelector('.categoria-gasto-nombre-error').style.display      = 'none';
    tr.querySelector('.categoria-gasto-tipo-select').value               = tr.dataset.tipo;
    tr.querySelector('.categoria-gasto-nombre-display').style.display    = '';
    tr.querySelector('.categoria-gasto-nombre-input').style.display      = 'none';
    tr.querySelector('.categoria-gasto-tipo-display').style.display      = '';
    tr.querySelector('.categoria-gasto-tipo-select').style.display       = 'none';
    tr.querySelector('.categoria-gasto-acciones-display').style.display  = 'flex';
    tr.querySelector('.categoria-gasto-acciones-edicion').style.display  = 'none';
}

function guardarCategoriaGastoInline(btn) {
    const tr             = btn.closest('tr');
    const input          = tr.querySelector('.categoria-gasto-nombre-input');
    const errorSpan      = tr.querySelector('.categoria-gasto-nombre-error');
    const tipoSelect     = tr.querySelector('.categoria-gasto-tipo-select');
    const nombreNuevo    = input.value.trim();
    const nombreOriginal = tr.dataset.nombre;
    const tipoNuevo      = tipoSelect.value;
    const dependientes   = parseInt(tr.dataset.dependientes, 10);
    const id             = tr.dataset.id;

    let error = '';
    if (!nombreNuevo)                                    error = CATEGORIA_GASTO_MSG_REQ;
    else if (nombreNuevo.length < CATEGORIA_GASTO_MIN)   error = CATEGORIA_GASTO_MSG_MIN;
    else if (nombreNuevo.length > CATEGORIA_GASTO_MAX)   error = CATEGORIA_GASTO_MSG_MAX;

    if (error) {
        errorSpan.textContent   = error;
        errorSpan.style.display = '';
        input.focus();
        return;
    }
    errorSpan.style.display = 'none';

    document.getElementById('editar-categoria-gasto-id-campo').value     = id;
    document.getElementById('editar-categoria-gasto-nombre-campo').value = nombreNuevo;
    document.getElementById('editar-categoria-gasto-tipo-campo').value   = tipoNuevo;

    const tipoLabel = tipoSelect.options[tipoSelect.selectedIndex].text;
    let cuerpo = `¿Guardar cambios en "${nombreOriginal}"?`;
    if (nombreNuevo !== nombreOriginal) cuerpo = `¿Renombrar "${nombreOriginal}" a "${nombreNuevo}" (${tipoLabel})?`;
    if (dependientes > 0) {
        const dep = dependientes === 1
            ? `1 gasto vinculado`
            : `${dependientes} gastos vinculados`;
        cuerpo += ` Tiene ${dep}.`;
    }
    document.getElementById('modal-confirmar-categoria-gasto-titulo').textContent        = 'Confirmar cambio';
    document.getElementById('modal-confirmar-categoria-gasto-cuerpo').textContent        = cuerpo;
    document.getElementById('modal-confirmar-categoria-gasto-footer-info').style.display    = 'none';
    document.getElementById('modal-confirmar-categoria-gasto-footer-confirm').style.display = 'flex';
    openModal('modal-confirmar-categoria-gasto');
}

function submitEditarCategoriaGasto() {
    document.getElementById('form-editar-categoria-gasto-hidden').submit();
}

// ── Delete modal ────────────────────────────────────────────────
function confirmarEliminarCategoriaGasto(btn) {
    const tr          = btn.closest('tr');
    const dependientes = parseInt(tr.dataset.dependientes, 10);
    // Si tiene gastos se deshabilitará (soft delete) — avisar al usuario
    const aviso = dependientes > 0
        ? ` Tiene ${dependientes} gasto${dependientes === 1 ? '' : 's'} vinculado${dependientes === 1 ? '' : 's'} — será deshabilitada en lugar de eliminada.`
        : '';
    abrirModalEliminar(
        'modal-eliminar-categoria-gasto',
        'categoria-gasto',
        tr.dataset.nombre,
        0,          // pasamos 0 para que abrirModalEliminar no agregue su propio texto de vinculados
        tr.dataset.id,
        '',
        aviso       // texto extra que se añade al cuerpo del modal
    );
}
