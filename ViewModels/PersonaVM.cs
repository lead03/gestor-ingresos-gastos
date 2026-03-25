using ControlGastos.Models;

namespace ControlGastos.ViewModels;

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
    // ── Fecha para mostrar (fecha de compra original) ─────────────────
    public int    Dia         { get; set; }
    public int    MesCompra   { get; set; }
    public int    AnioCompra  { get; set; }

    // ── Mes en que aparece (= mes de cierre si es TC cuota) ───────────
    public int    Mes  { get; set; }
    public int    Anio { get; set; }

    // ── Gasto ─────────────────────────────────────────────────────────
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
