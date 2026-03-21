using System.ComponentModel.DataAnnotations;
using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class IngresoFormVM
{
    public int Id   { get; set; }
    public int Mes  { get; set; } = DateTime.Today.Month;
    public int Anio { get; set; } = DateTime.Today.Year;
    public int Dia  { get; set; } = DateTime.Today.Day;

    public TipoIngreso Tipo { get; set; } = TipoIngreso.Propio;

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a $0.")]
    public decimal Monto { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    public int? CuentaId { get; set; }

    public List<CuentaResumenVM> Cuentas { get; set; } = new();
}

public class IngresoListVM
{
    public int  Mes  { get; set; }
    public int  Anio { get; set; }
    public List<Ingreso> Items { get; set; } = new();

    public decimal TotalPropio        { get; set; }
    public decimal TotalDistribuido   { get; set; }
    public decimal TotalCuentaPropia  { get; set; }
    public decimal TotalAhorro        { get; set; }
    public decimal TotalDpto          { get; set; }
    public decimal TotalUSS           { get; set; }
    public decimal TotalFIMA          { get; set; }
    public decimal TotalResto         { get; set; }
    public decimal Total => TotalPropio + TotalDistribuido + TotalCuentaPropia
                          + TotalAhorro + TotalDpto + TotalUSS + TotalFIMA + TotalResto;
}
