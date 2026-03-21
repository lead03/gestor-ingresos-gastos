using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class DashboardVM
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal Balance => TotalIngresos - TotalGastos;
    public decimal TotalGastosFijos { get; set; }
    public decimal TotalGastosVariables { get; set; }
    public decimal TotalGastosUsd { get; set; }

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

    public decimal? CotizacionDolar { get; set; }
}
