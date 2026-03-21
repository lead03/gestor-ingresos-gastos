namespace ControlGastos.Models;

public enum TipoCuenta
{
    Banco,
    Billetera,
    Efectivo
}

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

public enum TipoIngreso
{
    Propio,
    Distribuido,
    CuentaPropia,
    Ahorro,
    Dpto,
    USS,
    FIMA,
    Resto
}
