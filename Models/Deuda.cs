using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Deuda
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Persona { get; set; } = "";

    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // "MeDeben" | "LeDebo"
    public string Direccion { get; set; } = "MeDeben";

    // "Activa" | "Pagada" | "Parcial"
    public string Estado { get; set; } = "Activa";

    public decimal? MontoPagado { get; set; }
}
