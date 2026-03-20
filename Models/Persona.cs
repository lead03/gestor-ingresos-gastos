using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Persona
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = "";

    [MaxLength(200)]
    public string? Notas { get; set; }

    // Navegación
    public ICollection<Deuda>              Deudas        { get; set; } = new List<Deuda>();
    public ICollection<GastoParticipante>  Participaciones { get; set; } = new List<GastoParticipante>();
}
