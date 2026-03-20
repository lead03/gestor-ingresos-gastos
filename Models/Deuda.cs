using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Deuda
{
    public int Id { get; set; }

    // Persona declarada (opcional — puede ser deuda sin persona asignada)
    public int? PersonaId { get; set; }
    public Persona? Persona { get; set; }

    // Nombre libre (se usa si no hay PersonaId, o como fallback)
    [Required, MaxLength(80)]
    public string NombrePersona { get; set; } = "";

    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // "MeDeben" | "LeDebo"
    public string Direccion { get; set; } = "MeDeben";

    // "Activa" | "Pagada" | "Parcial"
    public string Estado { get; set; } = "Activa";

    public decimal? MontoPagado { get; set; }

    // Nombre a mostrar (usa Persona.Nombre si está vinculada)
    public string NombreMostrar => Persona?.Nombre ?? NombrePersona;
}
