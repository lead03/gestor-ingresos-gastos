using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Ingreso
{
    public int Id   { get; set; }
    public int Mes  { get; set; }
    public int Anio { get; set; }
    public int Dia  { get; set; }

    public TipoIngreso Tipo { get; set; } = TipoIngreso.Propio;

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a $0.")]
    public decimal Monto { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // Cuenta de origen (opcional)
    public int?    CuentaId { get; set; }
    public Cuenta? Cuenta   { get; set; }
}
