using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class DeudaCuota
{
    public int Id { get; set; }

    public int   DeudaId { get; set; }
    public Deuda Deuda   { get; set; } = null!;

    /// <summary>Mes al que corresponde la cuota (1–12).</summary>
    public int Mes  { get; set; }
    public int Anio { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Monto { get; set; }

    public decimal MontoPagado { get; set; }

    public EstadoDeuda Estado { get; set; } = EstadoDeuda.Activa;

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // Calculado — no persiste en DB
    public decimal Saldo => Monto - MontoPagado;
}
