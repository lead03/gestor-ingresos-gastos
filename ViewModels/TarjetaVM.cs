using System.ComponentModel.DataAnnotations;
using ControlGastos.Models;

namespace ControlGastos.ViewModels;

// ── Resumen del mes ───────────────────────────────────────────────────────
public class TarjetaResumenVM
{
    public int  Mes  { get; set; }
    public int  Anio { get; set; }
    public int? TarjetaIdFiltro { get; set; }

    public List<Tarjeta> Tarjetas { get; set; } = new();

    // Fechas mensuales por tarjeta (DiaCierre/DiaVencimiento del mes actual)
    // Clave: TarjetaId → fechas del mes; si no existe, usar Tarjeta.DiaCierre
    public Dictionary<int, TarjetaFechaMensual> FechasMensuales { get; set; } = new();

    // Helper: obtiene el DiaCierre efectivo para una tarjeta en este mes
    public int DiaCierreEfectivo(Tarjeta t)      => FechasMensuales.TryGetValue(t.Id, out var f) ? f.DiaCierre      : t.DiaCierre;
    public int DiaVencimientoEfectivo(Tarjeta t) => FechasMensuales.TryGetValue(t.Id, out var f) ? f.DiaVencimiento : t.DiaVencimiento;

    // Cuotas que cierran este mes, agrupadas por tarjeta
    public Dictionary<int, List<TarjetaCuota>> CuotasPorTarjeta { get; set; } = new();
    public Dictionary<int, decimal>            TotalPorTarjeta  { get; set; } = new();
    public decimal TotalGeneral { get; set; }

    // Todas las compras con cuotas pendientes (para el segundo tab)
    public List<CompraActivaVM> ComprasActivas { get; set; } = new();
}

// ── Una compra activa con sus cuotas pendientes ───────────────────────────
public class CompraActivaVM
{
    public int     TarjetaCuotaId { get; set; }
    public int     TarjetaId      { get; set; }
    public string  TarjetaNombre  { get; set; } = "";
    public string  Comercio       { get; set; } = "";
    public DateTime FechaCompra   { get; set; }
    public decimal MontoTotal     { get; set; }
    public decimal MontoCuota     { get; set; }
    public int     TotalCuotas    { get; set; }
    public int     CuotasPagadas  { get; set; }
    public int     CuotasRestantes => TotalCuotas - CuotasPagadas;
    public decimal SaldoRestante   => MontoCuota * CuotasRestantes;

    // Próximo cierre
    public int MesCierre  { get; set; }
    public int AnioCierre { get; set; }
    public string ProximoCierre => new DateTime(AnioCierre, MesCierre, 1).ToString("MMMM yyyy");
}

// ── Form gestión de tarjeta ───────────────────────────────────────────────
public class TarjetaFormVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(60)]
    public string Nombre { get; set; } = "";

    [MaxLength(30)]
    public string Banco { get; set; } = "";

    [MaxLength(20)]
    public string Red { get; set; } = "VISA";

    [Range(1, 31, ErrorMessage = "Día inválido.")]
    public int DiaCierre { get; set; } = 1;

    [Range(1, 31, ErrorMessage = "Día inválido.")]
    public int DiaVencimiento { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "El límite no puede ser negativo.")]
    public decimal? LimiteCredito { get; set; }
}

// ── Form fechas mensuales de una tarjeta ─────────────────────────────────
public class FechaMensualFormVM
{
    public int TarjetaId { get; set; }
    public int Mes       { get; set; }
    public int Anio      { get; set; }

    [Range(1, 31, ErrorMessage = "Día inválido.")]
    public int DiaCierre { get; set; } = 1;

    [Range(1, 31, ErrorMessage = "Día inválido.")]
    public int DiaVencimiento { get; set; } = 1;

    // Para mostrar en el form
    public string TarjetaNombre { get; set; } = "";
}

// ── Form cuota (sin cambios) ──────────────────────────────────────────────
public class TarjetaCuotaFormVM
{
    public int      Id            { get; set; }
    public int      TarjetaId     { get; set; }
    public string   Comercio      { get; set; } = "";
    public DateTime FechaCompra   { get; set; } = DateTime.Today;
    public decimal  MontoTotal    { get; set; }
    public int      TotalCuotas   { get; set; } = 1;
    public decimal  MontoCuota    { get; set; }
    public int      MesCierre     { get; set; } = DateTime.Today.Month;
    public int      AnioCierre    { get; set; } = DateTime.Today.Year;
    public int      CuotasPagadas { get; set; }
    public string   PagaParte     { get; set; } = "NO";
    public decimal? MontoPagoOtro { get; set; }
    public List<Tarjeta> Tarjetas { get; set; } = new();
}
