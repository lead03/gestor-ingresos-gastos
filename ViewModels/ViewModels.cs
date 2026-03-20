using ControlGastos.Models;

namespace ControlGastos.ViewModels;

// ─── Dashboard ────────────────────────────────────────────────────────────
public class DashboardVM
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal Balance => TotalIngresos - TotalGastos;
    public decimal TotalGastosFijos { get; set; }
    public decimal TotalGastosVariables { get; set; }

    // Por categoría (para gráfico torta)
    public List<(string Cat, decimal Total)> GastosPorCategoria { get; set; } = new();

    // Por día (para gráfico línea)
    public List<(int Dia, decimal Gastos, decimal Ingresos)> PorDia { get; set; } = new();

    // Últimos 6 meses (para gráfico barras)
    public List<(string Mes, decimal Gastos, decimal Ingresos)> Historico { get; set; } = new();

    // Cuentas
    public List<Cuenta> Cuentas { get; set; } = new();
    public decimal TotalCuentas { get; set; }
    public decimal TotalEfectivo { get; set; }

    // Deudas activas
    public decimal TotalMeDeben { get; set; }
    public List<Deuda> DeudasActivas { get; set; } = new();

    // Dólar (tipo de cambio)
    public decimal? CotizacionDolar { get; set; }
}

// ─── Gasto ────────────────────────────────────────────────────────────────
public class GastoFormVM
{
    public int Id { get; set; }
    public int Mes { get; set; } = DateTime.Today.Month;
    public int Anio { get; set; } = DateTime.Today.Year;
    public int Dia { get; set; } = DateTime.Today.Day;
    public int CategoriaId { get; set; }
    public decimal Monto { get; set; }
    public bool SeDivide { get; set; }
    public int? CantidadPersonas { get; set; }
    public decimal? MontoDividido { get; set; }
    public string? Descripcion { get; set; }
    public string? MedioPago { get; set; }
    public int? TarjetaCuotaId { get; set; }
    public List<CategoriaGasto> Categorias { get; set; } = new();
    public List<TarjetaCuota> Cuotas { get; set; } = new();
}

public class GastoListVM
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public List<GastoItem> Items { get; set; } = new();
    public decimal TotalFijos { get; set; }
    public decimal TotalVariables { get; set; }
    public decimal Total => TotalFijos + TotalVariables;

    // Agrupados por día
    public Dictionary<int, List<GastoItem>> PorDia { get; set; } = new();
}

// ─── Ingreso ──────────────────────────────────────────────────────────────
public class IngresoFormVM
{
    public int Id { get; set; }
    public int Mes { get; set; } = DateTime.Today.Month;
    public int Anio { get; set; } = DateTime.Today.Year;
    public int Dia { get; set; } = DateTime.Today.Day;
    public string Tipo { get; set; } = "Propio";
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }
    public string? Fuente { get; set; }
}

public class IngresoListVM
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public List<Ingreso> Items { get; set; } = new();

    // Totales por tipo
    public decimal TotalPropio { get; set; }
    public decimal TotalDistribuido { get; set; }
    public decimal TotalCuentaPropia { get; set; }
    public decimal TotalAhorro { get; set; }
    public decimal TotalDpto { get; set; }
    public decimal TotalUSS { get; set; }
    public decimal TotalFIMA { get; set; }
    public decimal TotalResto { get; set; }
    public decimal Total => TotalPropio + TotalDistribuido + TotalCuentaPropia
                          + TotalAhorro + TotalDpto + TotalUSS + TotalFIMA + TotalResto;
}

// ─── Tarjeta/Cuota ────────────────────────────────────────────────────────
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

    // Cuotas activas agrupadas por tarjeta
    public Dictionary<int, List<TarjetaCuota>> CuotasPorTarjeta { get; set; } = new();
    public Dictionary<int, decimal> TotalPorTarjeta { get; set; } = new();
    public decimal TotalGeneral { get; set; }

    // Fecha cierre/vencimiento del mes
    public Dictionary<int, (DateTime Cierre, DateTime Vencimiento)> FechasPorTarjeta { get; set; } = new();
}

// ─── Me deben ────────────────────────────────────────────────────────────
public class DeudaFormVM
{
    public int Id { get; set; }
    public string Persona { get; set; } = "";
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Today;
    public string? Descripcion { get; set; }
    public string Direccion { get; set; } = "MeDeben";
    public string Estado { get; set; } = "Activa";
    public decimal? MontoPagado { get; set; }
}

public class DeudaListVM
{
    public List<Deuda> MeDeben { get; set; } = new();
    public List<Deuda> LeDebo { get; set; } = new();
    public decimal TotalMeDeben { get; set; }
    public decimal TotalLeDebo { get; set; }
    public decimal Neto => TotalMeDeben - TotalLeDebo;
}

// ─── Efectivo ────────────────────────────────────────────────────────────
public class EfectivoVM
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public List<EfectivoDesglose> Desgloses { get; set; } = new();
    public decimal Total => Desgloses.Sum(d => d.Total);
    public static int[] Denominaciones = { 10, 20, 50, 100, 200, 500, 1000, 2000 };
}
