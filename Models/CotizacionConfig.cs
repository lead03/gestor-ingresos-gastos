using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class CotizacionConfig
{
    public int Id { get; set; }  // siempre Id=1, singleton

    [Required, MaxLength(20)]
    public string TipoDolar { get; set; } = "blue";  // blue | oficial | tarjeta | bolsa

    [Required, MaxLength(200)]
    public string ApiUrl { get; set; } = "https://dolarapi.com/v1/dolares";

    public bool UsarApi { get; set; } = true;

    public decimal CotizacionManual { get; set; } = 0;

    // Cache de última respuesta
    public decimal? UltimoValor { get; set; }
    public string? UltimoTipo { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
}
