/**
 * money-input.js — Formateo de inputs de dinero en cultura es-AR
 *
 * setupMoneyInput(el, onChangeCb?)
 *   Adjunta formateo en vivo a un <input>. Requiere que el elemento tenga
 *   data-init-value con el valor numérico inicial (ej: "1234.56" o "0").
 *
 * getRawMoney(el)
 *   Devuelve el valor numérico de un input formateado (puntos como miles, coma decimal).
 *
 * restoreMoneyForSubmit(el)
 *   Convierte el valor formateado a número plano ("1.234,56" → "1234.56") antes del POST.
 *   Uso: campos obligatorios no-nullable (nunca quedan vacíos, mínimo "0").
 *
 * restoreOptionalMoneyForSubmit(el)
 *   Igual que restoreMoneyForSubmit, pero si el valor es 0 deja la cadena vacía.
 *   Uso: campos nullable opcionales (vacío = null en el server).
 */

function getRawMoney(el) {
    return parseFloat((el.value || '').replace(/\./g, '').replace(',', '.')) || 0;
}

function restoreMoneyForSubmit(el) {
    el.value = getRawMoney(el).toString();
}

function restoreOptionalMoneyForSubmit(el) {
    const raw = getRawMoney(el);
    el.value = raw > 0 ? raw.toString() : '';
}

function setupMoneyInput(el, onChangeCb) {
    el.type = 'text';
    el.setAttribute('inputmode', 'decimal');

    el.addEventListener('keydown', function(e) {
        const allowed = ['Backspace', 'Delete', 'Tab', 'Escape', 'Enter',
                         'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
        if (allowed.includes(e.key) || e.ctrlKey || e.metaKey) return;

        if (/^\d$/.test(e.key)) {
            // Bloquear 3er decimal
            const commaIdx = el.value.indexOf(',');
            if (commaIdx !== -1) {
                const selStart = el.selectionStart;
                const selEnd   = el.selectionEnd;
                if (selStart > commaIdx) {
                    const charsAfterComma = el.value.substring(commaIdx + 1, selStart);
                    if (charsAfterComma.length >= 2 && selStart === selEnd) {
                        e.preventDefault(); return;
                    }
                }
            }
            return;
        }

        // Coma y punto (numpad) — ambos actúan como separador decimal
        if (e.key === ',' || e.key === '.') {
            const textBefore = el.value.substring(0, el.selectionStart);
            const textAfter  = el.value.substring(el.selectionEnd);
            if ((textBefore + textAfter).includes(',')) { e.preventDefault(); return; }
            if (e.key === '.') {
                e.preventDefault();
                el.value = textBefore + ',' + textAfter;
                el.setSelectionRange(textBefore.length + 1, textBefore.length + 1);
                el.dispatchEvent(new Event('input'));
            }
            return;
        }

        e.preventDefault();
    });

    el.addEventListener('focus', function() {
        if (getRawMoney(el) === 0) setTimeout(() => el.select(), 0);
    });

    el.addEventListener('input', function() {
        const cursor    = el.selectionStart;
        const oldVal    = el.value;
        const dotsAntes = (oldVal.substring(0, cursor).match(/\./g) || []).length;

        const parts  = oldVal.replace(/\./g, '').split(',');
        let intStr   = parts[0].replace(/\D/g, '');
        const decStr = parts.length > 1 ? parts[1].substring(0, 2) : null;

        if (parseInt(intStr || '0', 10) > 9999999999) intStr = '9999999999';

        const intFmt = intStr ? intStr.replace(/\B(?=(\d{3})+(?!\d))/g, '.') : '';
        const newVal = decStr !== null ? `${intFmt},${decStr}` : intFmt;
        el.value = newVal;

        const subNew      = newVal.substring(0, cursor);
        const dotsDespues = (subNew.match(/\./g) || []).length;
        const newCursor   = Math.min(cursor + (dotsDespues - dotsAntes), newVal.length);
        try { el.setSelectionRange(newCursor, newCursor); } catch (_) {}

        if (onChangeCb) onChangeCb();
    });

    el.addEventListener('blur', function() {
        const raw = getRawMoney(el);
        if (raw > 0) {
            el.value = raw.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        }
        if (onChangeCb) onChangeCb();
    });

    // Valor inicial: si es > 0 formatea; si es 0 limpia para mostrar el placeholder
    const initVal = parseFloat(el.dataset.initValue || el.value) || 0;
    if (initVal > 0) {
        el.value = initVal.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    } else {
        el.value = '';
    }
}
