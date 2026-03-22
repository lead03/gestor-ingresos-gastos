using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class IngresoDistribucion
{
    public int Id { get; set; }

    public int IngresoId { get; set; }
    public Ingreso Ingreso { get; set; } = null!;

    public int CuentaId { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public decimal Monto { get; set; }
}
