using ControlGastos.Models;

namespace ControlGastos.ViewModels;

// ── Movimiento unificado (pre-calculado por el servicio) ──────────────────
public class MovimientoPersonaVM
{
    public TipoMovimientoPersona TipoMov     { get; set; }
    public DateTime              Fecha       { get; set; }
    public string                Categoria   { get; set; } = "—";
    public string?               Descripcion { get; set; }
    public decimal               Monto       { get; set; }   // siempre positivo
    public int                   RefId       { get; set; }   // GastoItemId | PagoId | DeudaId
    /// <summary>Solo para DeudaLe/DeudaMe auto-generadas desde un Gasto.</summary>
    public int?                  GastoItemId { get; set; }

    // String para data-tipo en HTML — debe coincidir con TipoMov en tipos-movimiento.js
    public string Tipo => TipoMov == TipoMovimientoPersona.Credito ? "Crédito" : TipoMov.ToString();

    // +1 si aumenta lo que me deben, -1 si lo reduce
    public int     Signo    => TipoMov is TipoMovimientoPersona.Gasto
                                        or TipoMovimientoPersona.Credito
                                        or TipoMovimientoPersona.DeudaMe ? 1 : -1;
    public decimal MontoNeto => Monto * Signo;
    public string  Color    => Signo >= 0 ? "green" : "amber";
    public string  Prefijo  => Signo >= 0 ? "+" : "−";
}

// ── Formulario pago recibido ──────────────────────────────────────────────
public class PagoPersonaFormVM
{
    public int     PagoId      { get; set; }   // 0 = nuevo
    public int     PersonaId   { get; set; }
    public int     CuentaId    { get; set; }
    public decimal Monto       { get; set; }
    public int     Mes         { get; set; }
    public int     Anio        { get; set; }
    public int     Dia         { get; set; }
    public Moneda  Moneda      { get; set; } = Moneda.ARS;
    public string? Descripcion { get; set; }
}

// ── Lista de personas ─────────────────────────────────────────────────────
public class PersonaListVM
{
    public List<PersonaResumenVM> Personas { get; set; } = new();
}

public class PersonaResumenVM
{
    public int    Id     { get; set; }
    public string Nombre { get; set; } = "";
    public string? Notas { get; set; }

    // Cuántos gastos compartidos tiene
    public int TotalMovimientos { get; set; }

    // Balance: positivo = me debe, negativo = le debo, 0 = saldado
    public decimal Balance { get; set; }
    public string  EstadoBalance =>
        Balance > 0 ? $"Me debe $ {Balance:N2}" :
        Balance < 0 ? $"Le debo $ {Math.Abs(Balance):N2}" :
        "Saldado";
    public string ClaseBalance =>
        Balance > 0 ? "green" : Balance < 0 ? "red" : "accent";
}

// ── Detalle de una persona ────────────────────────────────────────────────
public class PersonaDetalleVM
{
    public Persona Persona { get; set; } = null!;
    public decimal Balance { get; set; }
    public string  EstadoBalance =>
        Balance > 0 ? $"Me debe $ {Balance:N2}" :
        Balance < 0 ? $"Le debo $ {Math.Abs(Balance):N2}" :
        "Saldado";
    public string ClaseBalance =>
        Balance > 0 ? "green" : Balance < 0 ? "red" : "accent";

    // Total de compras (all-time, para el KPI)
    public int TotalMovimientos { get; set; }

    // Movimientos del mes seleccionado (ya filtrados)
    public List<PersonaMesVM> PorMes { get; set; } = new();

    // Deudas directas
    public List<Deuda> Deudas { get; set; } = new();
    public decimal TotalDeudas { get; set; }

    // Pagos recibidos
    public List<PagoPersona> Pagos { get; set; } = new();
    public decimal TotalPagos { get; set; }

    // Movimientos del mes — pre-calculados por el servicio (signos, colores incluidos)
    public List<MovimientoPersonaVM> Movimientos { get; set; } = new();
}

public class PersonaMesVM
{
    public int    Mes  { get; set; }
    public int    Anio { get; set; }
    public string Label => new DateTime(Anio, Mes, 1).ToString("MMMM yyyy");
    public decimal Total { get; set; }
    public List<PersonaParticipacionVM> Participaciones { get; set; } = new();
}

/// <summary>
/// Representa una línea en el historial de gastos compartidos de una persona.
/// Para gastos de TC en cuotas, se genera una entrada POR cuota (expandida).
/// </summary>
public class PersonaParticipacionVM
{
    // ── Fecha de compra original ──────────────────────────────────────
    public DateTime FechaCompra { get; set; }

    // ── Mes en que aparece (= mes de cierre si es TC cuota) ───────────
    public int    Mes  { get; set; }
    public int    Anio { get; set; }

    // ── Gasto ─────────────────────────────────────────────────────────
    public int     GastoItemId { get; set; }
    public string  Categoria   { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Monto       { get; set; }   // parte del mes (total / n cuotas)

    // ── Info de cuota TC (null si es gasto directo) ───────────────────
    public int? NumeroCuota { get; set; }
    public int? TotalCuotas { get; set; }
    public bool EsCuota => NumeroCuota.HasValue;
}

// ── Formulario alta/edición ───────────────────────────────────────────────
public class PersonaFormVM
{
    public int     Id     { get; set; }
    public string  Nombre { get; set; } = "";
    public string? Notas  { get; set; }
}
