using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

/// <summary>
/// Opciones configurables para desplegables (Red de tarjeta, Banco, Billetera, etc.)
/// </summary>
public class ConfigOpcion
{
    public int Id { get; set; }

    // "Red" | "Banco" | "Billetera" | "Setting:xxx"
    [MaxLength(30)]
    public string Tipo { get; set; } = "";

    // FK a TipoEntidad — solo para Banco (1) y Billetera (2); null para Red y Settings
    public int? TipoEntidadId { get; set; }
    public TipoEntidad? TipoEntidad { get; set; }

    [Required, MaxLength(60)]
    public string Valor { get; set; } = "";

    public int Orden { get; set; }
}
