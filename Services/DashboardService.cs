using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class DashboardService(
    IGastoRepository   gastoRepo,
    IIngresoRepository ingresoRepo,
    ICuentaRepository  cuentaRepo,
    IDeudaRepository   deudaRepo,
    CotizacionService  cotizacionSvc)
{
    public async Task<DashboardVM> GetDashboardAsync(int mes, int anio)
    {
        var gastos   = await gastoRepo.GetByMesAsync(mes, anio);
        var ingresos = await ingresoRepo.GetByMesAsync(mes, anio);
        var cuentas  = await cuentaRepo.GetAllActivasAsync();
        var deudas   = (await deudaRepo.GetAllAsync())
                           .Where(d => d.Estado == EstadoDeuda.Activa && d.Direccion == DireccionDeuda.MeDeben)
                           .ToList();

        // Obtener cotización primero para usarla en todos los cálculos
        var cotizacion = await cotizacionSvc.GetCotizacionAsync();

        // Convierte USD → ARS si hay cotización; si no, usa 0 (el gasto en USD no impacta)
        decimal MontoEfectivo(GastoItem g) =>
            g.Moneda == "USD"
                ? g.MiParteMes * (cotizacion ?? 0m)
                : g.MiParteMes;

        var vm = new DashboardVM
        {
            Mes   = mes, Anio = anio,
            TotalGastos          = gastos.Sum(MontoEfectivo),
            TotalIngresos        = ingresos.Sum(i => i.Monto),
            TotalGastosFijos     = gastos.Where(g => g.Categoria.Tipo == "Fijo").Sum(MontoEfectivo),
            TotalGastosVariables = gastos.Where(g => g.Categoria.Tipo == "Variable").Sum(MontoEfectivo),
            TotalGastosUsd       = gastos.Where(g => g.Moneda == "USD").Sum(g => g.MiParteMes), // monto bruto en USD
            CotizacionDolar      = cotizacion,
            Cuentas      = cuentas,
            TotalCuentas = cuentas.Sum(c => c.SaldoInicial),
            TotalMeDeben = deudas.Sum(d => d.Monto - (d.MontoPagado ?? 0)),
            DeudasActivas= deudas,
            GastosPorCategoria = gastos
                .GroupBy(g => g.Categoria.Nombre)
                .Select(grp => (grp.Key, grp.Sum(MontoEfectivo)))
                .OrderByDescending(x => x.Item2)
                .ToList(),
            PorDia = Enumerable.Range(1, DateTime.DaysInMonth(anio, mes))
                .Select(d => (d,
                    gastos.Where(g => g.Dia == d).Sum(MontoEfectivo),
                    ingresos.Where(i => i.Dia == d).Sum(i => i.Monto)
                )).ToList()
        };

        var historico = new List<(string, decimal, decimal)>();
        for (int offset = 5; offset >= 0; offset--)
        {
            int m = mes - offset; int a = anio;
            if (m <= 0) { m += 12; a--; }
            var gM = await gastoRepo.GetByMesAsync(m, a);
            var iM = await ingresoRepo.GetByMesAsync(m, a);
            // En el histórico también convertimos USD → ARS para que el gráfico sea homogéneo
            historico.Add((new DateTime(a, m, 1).ToString("MMM"), gM.Sum(MontoEfectivo), iM.Sum(i => i.Monto)));
        }
        vm.Historico = historico;
        return vm;
    }
}
