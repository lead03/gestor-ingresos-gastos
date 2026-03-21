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

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a $0.")]
    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    public DireccionDeuda Direccion { get; set; } = DireccionDeuda.MeDeben;
    public EstadoDeuda    Estado    { get; set; } = EstadoDeuda.Activa;

    public decimal? MontoPagado { get; set; }

    public string NombreMostrar => Persona?.Nombre ?? NombrePersona;
}
