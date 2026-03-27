namespace ControlGastos.Models;

public enum TipoEntidad { Efectivo, Banco, Billetera }

public enum TipoParticipante
{
    Yo,
    Persona,
    Pagado
}

public enum DireccionDeuda
{
    MeDeben,
    LeDebo
}

public enum EstadoDeuda
{
    Activa,
    Pagada,
    Parcial
}

/// <summary>
/// Tipo de movimiento en el detalle de una Persona.
/// IMPORTANTE: los valores ToString() deben coincidir con TipoMov en tipos-movimiento.js
/// excepto Credito → "Crédito" (acento), que se mapea explícitamente.
/// </summary>
public enum TipoMovimientoPersona
{
    Gasto,
    Credito,   // string HTML: "Crédito"
    Pago,
    DeudaMe,
    DeudaLe
}

/// <summary>
/// Tipo de movimiento en el detalle de una Cuenta.
/// Debe coincidir con TipoCuentaMov en tipos-movimiento.js
/// </summary>
public enum TipoMovimientoCuenta
{
    Gasto,
    Ingreso
}

