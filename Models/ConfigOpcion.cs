using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

/// <summary>
/// Opciones configurables para desplegables (Red de tarjeta, Banco/Emisor, etc.)
/// </summary>
public class ConfigOpcion
{
    public int Id { get; set; }

    // "Red" | "Banco"
    [MaxLength(30)]
    public string Tipo { get; set; } = "";

    [Required, MaxLength(60)]
    public string Valor { get; set; } = "";

    public int Orden { get; set; }
}
