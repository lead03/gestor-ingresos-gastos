using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Deuda
{
    public int Id { get; set; }

    public int?     PersonaId { get; set; }
    public Persona? Persona   { get; set; }

    [Required(ErrorMessage = "El nombre de la persona es obligatorio.")]
    [MaxLength(80)]
    public string NombrePersona { get; set; } = "";

    [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    public DireccionDeuda Direccion { get; set; } = DireccionDeuda.MeDeben;
    public EstadoDeuda    Estado    { get; set; } = EstadoDeuda.Activa;

    public decimal? MontoPagado { get; set; }

    /// <summary>
    /// Indica que esta deuda es una cuenta de crédito mensual:
    /// el saldo real se compone de cuotas (<see cref="Cuotas"/>), no de <see cref="Monto"/>.
    /// </summary>
    public bool AceptaCuotas { get; set; }

    /// <summary>Vincula esta deuda a un GastoItem cuando fue auto-generada por el campo "¿Quién pagó?".</summary>
    public int? GastoItemId { get; set; }

    public ICollection<DeudaCuota> Cuotas { get; set; } = new List<DeudaCuota>();

    public string NombreMostrar => Persona?.Nombre ?? NombrePersona;
}
