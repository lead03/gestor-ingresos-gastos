/**
 * tipos-movimiento.js
 *
 * Constantes de tipos y filtros de movimientos de personas.
 * DEBEN coincidir con los enums del backend:
 *   - TipoMovimientoPersona  (Models/Enums.cs)
 *   - TipoMovimientoCuenta   (Models/Enums.cs)
 *
 * La propiedad Tipo de MovimientoPersonaVM genera el string HTML
 * usando el mismo mapeo que aquí (Credito → "Crédito" con acento).
 */

'use strict';

/** Tipos de movimiento de Persona — coinciden con MovimientoPersonaVM.Tipo */
const TipoMov = Object.freeze({
    Gasto:   'Gasto',
    Credito: 'Crédito',
    Pago:    'Pago',
    DeudaMe: 'DeudaMe',
    DeudaLe: 'DeudaLe'
});

/** Filtros disponibles en la tabla de movimientos de Persona */
const FiltroMov = Object.freeze({
    Todos:      'todos',
    Gasto:      'Gasto',
    Credito:    'Crédito',
    Pago:       'Pago',
    Deuda:      'Deuda',      // agrupa DeudaMe + DeudaLe
    SinCredito: 'sin-credito' // excluye Crédito (cuotas TC)
});

/** Tipos de movimiento de Cuenta — coinciden con TipoMovimientoCuenta */
const TipoCuentaMov = Object.freeze({
    Gasto:   'Gasto',
    Ingreso: 'Ingreso'
});
