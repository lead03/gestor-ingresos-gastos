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

    // Si el gasto se divide entre participantes
    public bool SeDivide { get; set; }

    // Mi parte final (calculada de los participantes tipo "Yo")
    public decimal? MiParte { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    [MaxLength(50)]
    public string? MedioPago { get; set; }

    public int? TarjetaCuotaId { get; set; }
    public TarjetaCuota? TarjetaCuota { get; set; }

    // Participantes del gasto dividido
    public ICollection<GastoParticipante> Participantes { get; set; } = new List<GastoParticipante>();
}
