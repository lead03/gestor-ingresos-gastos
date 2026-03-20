using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class DashboardService(
    IGastoRepository   gastoRepo,
    IIngresoRepository ingresoRepo,
    ICuentaRepository  cuentaRepo,
    IDeudaRepository   deudaRepo)
{
    public async Task<DashboardVM> GetDashboardAsync(int mes, int anio)
    {
        var gastos   = await gastoRepo.GetByMesAsync(mes, anio);
        var ingresos = await ingresoRepo.GetByMesAsync(mes, anio);
        var cuentas  = await cuentaRepo.GetByMesAsync(mes, anio);
        var deudas   = (await deudaRepo.GetAllAsync())
                           .Where(d => d.Estado == "Activa" && d.Direccion == "MeDeben")
                           .ToList();

        decimal MontoEfectivo(GastoItem g) =>
            g.SeDivide ? g.MiParte ?? g.Monto : g.Monto;

        var vm = new DashboardVM
        {
            Mes   = mes, Anio = anio,
            TotalGastos          = gastos.Sum(MontoEfectivo),
            TotalIngresos        = ingresos.Sum(i => i.Monto),
            TotalGastosFijos     = gastos.Where(g => g.Categoria.Tipo == "Fijo").Sum(MontoEfectivo),
            TotalGastosVariables = gastos.Where(g => g.Categoria.Tipo == "Variable").Sum(MontoEfectivo),
            Cuentas      = cuentas,
            TotalCuentas = cuentas.Sum(c => c.SaldoFinal),
            TotalMeDeben = deudas.Sum(d => d.Monto - (d.MontoPagado ?? 0)),
            DeudasActivas= deudas,
            GastosPorCategoria = gastos
                .GroupBy(g => g.Categoria.Nombre)
                .Select(grp => (grp.Key, grp.Sum(MontoEfectivo)))
                .OrderByDescending(x => x.Item2)
                .ToList(),
            PorDia = Enumerable.Range(1, DateTime.DaysInMonth(anio, mes))
                .Select(d => (
                    d,
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
            historico.Add((
                new DateTime(a, m, 1).ToString("MMM"),
                gM.Sum(MontoEfectivo),
                iM.Sum(i => i.Monto)
            ));
        }
        vm.Historico = historico;

        return vm;
    }
}
