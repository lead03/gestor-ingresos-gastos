using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Ingreso
{
    public int Id   { get; set; }
    public int Mes  { get; set; }
    public int Anio { get; set; }
    public int Dia  { get; set; }

    // Tipo por FK (reemplaza el enum)
    public int TipoIngresoId { get; set; }
    public TipoIngreso TipoIngreso { get; set; } = null!;

    public Moneda Moneda { get; set; } = Moneda.ARS;

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a $0.")]
    public decimal Monto { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // Distribución por cuentas (reemplaza single CuentaId)
    public ICollection<IngresoDistribucion> Distribuciones { get; set; } = new List<IngresoDistribucion>();
}
