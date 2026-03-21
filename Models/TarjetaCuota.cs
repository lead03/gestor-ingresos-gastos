using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class TarjetaCuota
{
    public int Id { get; set; }
    public int TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;

    [Required, MaxLength(150)]
    public string Comercio { get; set; } = "";

    public DateTime FechaCompra { get; set; }
    public decimal MontoTotal { get; set; }
    public int TotalCuotas { get; set; }
    public decimal MontoCuota { get; set; }

    // Mes en que aparece en el resumen
    public int MesCierre { get; set; }
    public int AnioCierre { get; set; }

    public int CuotasPagadas { get; set; }

    // Enlace al GastoItem que originó esta cuota (para navegar desde el resumen de tarjeta)
    public int? GastoItemId { get; set; }

    // Propiedad calculada — no se persiste en la DB
    public int CuotasRestantes => TotalCuotas - CuotasPagadas;

    // "NO" | "DEBE"
    [MaxLength(10)]
    public string PagaParte { get; set; } = "NO";

    public decimal? MontoPagoOtro { get; set; }

    public ICollection<GastoItem> GastosAsociados { get; set; } = new List<GastoItem>();
}
