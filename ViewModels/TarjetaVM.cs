using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class TarjetaCuotaFormVM
{
    public int Id { get; set; }
    public int TarjetaId { get; set; }
    public string Comercio { get; set; } = "";
    public DateTime FechaCompra { get; set; } = DateTime.Today;
    public decimal MontoTotal { get; set; }
    public int TotalCuotas { get; set; } = 1;
    public decimal MontoCuota { get; set; }
    public int MesCierre { get; set; } = DateTime.Today.Month;
    public int AnioCierre { get; set; } = DateTime.Today.Year;
    public int CuotasPagadas { get; set; }
    public string PagaParte { get; set; } = "NO";
    public decimal? MontoPagoOtro { get; set; }
    public List<Tarjeta> Tarjetas { get; set; } = new();
}

public class TarjetaResumenVM
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public List<Tarjeta> Tarjetas { get; set; } = new();
    public Dictionary<int, List<TarjetaCuota>> CuotasPorTarjeta { get; set; } = new();
    public Dictionary<int, decimal> TotalPorTarjeta { get; set; } = new();
    public decimal TotalGeneral { get; set; }
    public Dictionary<int, (DateTime Cierre, DateTime Vencimiento)> FechasPorTarjeta { get; set; } = new();
}
