using ControlGastos.Common;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class CuentaService(ICuentaRepository repo)
{
    public async Task<CuentaListVM> GetListAsync()
    {
        var cuentas = await repo.GetAllActivasAsync();
        var resumen = new List<CuentaResumenVM>();

        foreach (var c in cuentas)
        {
            var saldo = await CalcularSaldoAsync(c);
            resumen.Add(new CuentaResumenVM
            {
                Id                 = c.Id,
                Nombre             = c.Nombre,
                Tipo               = c.Tipo,
                SaldoInicial       = c.SaldoInicial,
                SaldoActual  = saldo,
                AlertaSaldo  = c.AlertaSaldo,
                Moneda       = c.Moneda,
            });
        }

        return new CuentaListVM { Cuentas = resumen };
    }

    public async Task<CuentaDetalleVM?> GetDetalleAsync(int id)
    {
        var cuenta = await repo.GetByIdAsync(id);
        if (cuenta == null) return null;

        var gastos   = await repo.GetGastosByCuentaAsync(id);
        var ingresos = await repo.GetIngresosByCuentaAsync(id);

        var movimientos = new List<MovimientoCuentaVM>();

        movimientos.AddRange(gastos.Select(g => new MovimientoCuentaVM
        {
            Fecha       = new DateTime(g.Anio, g.Mes, g.Dia),
            Descripcion = g.Descripcion ?? g.Categoria.Nombre,
            Categoria   = g.Categoria.Nombre,
            Tipo        = "Gasto",
            Monto       = g.SeDivide ? g.MiParte ?? g.Monto : g.Monto
        }));

        movimientos.AddRange(ingresos.Select(i => new MovimientoCuentaVM
        {
            Fecha       = new DateTime(i.Anio, i.Mes, i.Dia),
            Descripcion = i.Descripcion ?? i.TipoIngreso?.Nombre ?? "Ingreso",
            Categoria   = i.TipoIngreso?.Nombre ?? "Ingreso",
            Tipo        = "Ingreso",
            Monto       = i.Monto
        }));

        movimientos = movimientos.OrderByDescending(m => m.Fecha).ToList();

        var saldoActual = cuenta.SaldoInicial
                        + ingresos.Sum(i => i.Monto)
                        - gastos.Sum(g => g.SeDivide ? g.MiParte ?? g.Monto : g.Monto);

        cuenta.SaldoActual = saldoActual;

        return new CuentaDetalleVM
        {
            Cuenta      = cuenta,
            SaldoActual = saldoActual,
            Movimientos = movimientos
        };
    }

    public async Task<List<CuentaResumenVM>> GetCuentasConSaldoAsync()
    {
        var cuentas = await repo.GetAllActivasAsync();
        var resultado = new List<CuentaResumenVM>();

        foreach (var c in cuentas)
        {
            var saldo = await CalcularSaldoAsync(c);
            resultado.Add(new CuentaResumenVM
            {
                Id                 = c.Id,
                Nombre             = c.Nombre,
                Tipo               = c.Tipo,
                SaldoActual  = saldo,
                AlertaSaldo  = c.AlertaSaldo,
                Moneda       = c.Moneda,
            });
        }
        return resultado;
    }

    public async Task<Result> SaveAsync(CuentaFormVM vm)
    {
        if (vm.Id == 0)
        {
            await repo.AddAsync(new Cuenta
            {
                Nombre       = vm.Nombre,
                Tipo         = vm.Tipo,
                Moneda       = vm.Moneda,
                SaldoInicial = vm.SaldoInicial,
                AlertaSaldo  = vm.AlertaSaldo,
                Activa       = true
            });
        }
        else
        {
            var c = await repo.GetByIdAsync(vm.Id);
            if (c == null) return Result.Fail("Cuenta no encontrada.");

            c.Nombre       = vm.Nombre;
            c.Tipo         = vm.Tipo;
            c.Moneda       = vm.Moneda;
            c.SaldoInicial = vm.SaldoInicial;
            c.AlertaSaldo  = vm.AlertaSaldo;
            c.Activa       = vm.Activa;

            await repo.UpdateAsync(c);
        }
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var gastos   = await repo.GetGastosByCuentaAsync(id);
        var ingresos = await repo.GetIngresosByCuentaAsync(id);

        if (gastos.Any() || ingresos.Any())
        {
            await repo.DeleteAsync(id); // soft delete
            return Result.Ok();
        }

        await repo.DeleteAsync(id);
        return Result.Ok();
    }

    // ── Interno ────────────────────────────────────────────────────────
    private async Task<decimal> CalcularSaldoAsync(Cuenta c)
    {
        var gastos   = await repo.GetGastosByCuentaAsync(c.Id);
        var ingresos = await repo.GetIngresosByCuentaAsync(c.Id);

        return c.SaldoInicial
             + ingresos.Sum(i => i.Monto)
             - gastos.Sum(g => g.SeDivide ? g.MiParte ?? g.Monto : g.Monto);
    }
}
