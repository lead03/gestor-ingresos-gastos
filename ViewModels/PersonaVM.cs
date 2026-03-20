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

    // Movimientos agrupados por mes
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
    public List<GastoParticipante> Participaciones { get; set; } = new();
}

// ── Formulario alta/edición ───────────────────────────────────────────────
public class PersonaFormVM
{
    public int     Id     { get; set; }
    public string  Nombre { get; set; } = "";
    public string? Notas  { get; set; }
}
