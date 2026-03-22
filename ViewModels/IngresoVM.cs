using System.ComponentModel.DataAnnotations;
using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class DistribucionFormVM
{
    public int     CuentaId { get; set; }
    public decimal Monto    { get; set; }
}

public class IngresoFormVM
{
    public int Id   { get; set; }
    public int Mes  { get; set; } = DateTime.Today.Month;
    public int Anio { get; set; } = DateTime.Today.Year;
    public int Dia  { get; set; } = DateTime.Today.Day;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de ingreso.")]
    public int TipoIngresoId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a $0.")]
    public decimal Monto { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    public List<DistribucionFormVM> Distribuciones { get; set; } = new();

    // Listas para selects
    public List<TipoIngreso>     Tipos   { get; set; } = new();
    public List<CuentaResumenVM> Cuentas { get; set; } = new();

    // ¿Hay cuentas declaradas?
    public bool HayCuentas => Cuentas.Any();
}

public class IngresoListVM
{
    public int  Mes  { get; set; }
    public int  Anio { get; set; }
    public List<Ingreso> Items { get; set; } = new();
    public List<(string NombreTipo, decimal Total)> TotalesPorTipo { get; set; } = new();
    public decimal Total => TotalesPorTipo.Sum(x => x.Total);
}
