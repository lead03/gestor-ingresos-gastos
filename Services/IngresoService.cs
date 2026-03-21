using ControlGastos.Common;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class IngresoService(IIngresoRepository repo, ICuentaRepository cuentaRepo)
{
    public async Task<IngresoListVM> GetListAsync(int mes, int anio)
    {
        var items = await repo.GetByMesAsync(mes, anio);

        return new IngresoListVM
        {
            Mes = mes, Anio = anio, Items = items,
            TotalPropio       = items.Where(i => i.Tipo == TipoIngreso.Propio).Sum(i => i.Monto),
            TotalDistribuido  = items.Where(i => i.Tipo == TipoIngreso.Distribuido).Sum(i => i.Monto),
            TotalCuentaPropia = items.Where(i => i.Tipo == TipoIngreso.CuentaPropia).Sum(i => i.Monto),
            TotalAhorro       = items.Where(i => i.Tipo == TipoIngreso.Ahorro).Sum(i => i.Monto),
            TotalDpto         = items.Where(i => i.Tipo == TipoIngreso.Dpto).Sum(i => i.Monto),
            TotalUSS          = items.Where(i => i.Tipo == TipoIngreso.USS).Sum(i => i.Monto),
            TotalFIMA         = items.Where(i => i.Tipo == TipoIngreso.FIMA).Sum(i => i.Monto),
            TotalResto        = items.Where(i => i.Tipo == TipoIngreso.Resto).Sum(i => i.Monto),
        };
    }

    public async Task<IngresoFormVM> GetFormAsync()
    {
        var cuentas = await cuentaRepo.GetAllActivasAsync();
        return new IngresoFormVM
        {
            Cuentas = cuentas.Select(c => new CuentaResumenVM
            {
                Id     = c.Id,
                Nombre = c.Nombre,
                Tipo   = c.Tipo
            }).ToList()
        };
    }

    public async Task<Result> SaveAsync(IngresoFormVM vm)
    {
        if (vm.Id == 0)
        {
            await repo.AddAsync(new Ingreso
            {
                Mes = vm.Mes, Anio = vm.Anio, Dia = vm.Dia,
                Tipo = vm.Tipo, Monto = vm.Monto,
                Descripcion = vm.Descripcion,
                CuentaId = vm.CuentaId
            });
        }
        else
        {
            var ingreso = await repo.GetByIdAsync(vm.Id);
            if (ingreso == null) return Result.Fail($"Ingreso {vm.Id} no encontrado.");

            ingreso.Mes = vm.Mes; ingreso.Anio = vm.Anio; ingreso.Dia = vm.Dia;
            ingreso.Tipo = vm.Tipo; ingreso.Monto = vm.Monto;
            ingreso.Descripcion = vm.Descripcion;
            ingreso.CuentaId = vm.CuentaId;

            await repo.UpdateAsync(ingreso);
        }
        return Result.Ok();
    }

    public Task DeleteAsync(int id) => repo.DeleteAsync(id);
}
