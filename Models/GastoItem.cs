using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class GastoItem
{
    public int Id { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public int Dia { get; set; }

    public int CategoriaId { get; set; }
    public CategoriaGasto Categoria { get; set; } = null!;

    [Required]
    public decimal Monto { get; set; }

    // Si el monto se divide entre varias personas
    public bool SeDivide { get; set; }
    public decimal? MontoDividido { get; set; }
    public int? CantidadPersonas { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // "Efectivo" | "Galicia" | "Santander" | etc.
    [MaxLength(50)]
    public string? MedioPago { get; set; }

    // Si corresponde a una cuota de tarjeta
    public int? TarjetaCuotaId { get; set; }
    public TarjetaCuota? TarjetaCuota { get; set; }
}
