using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class IngresoService(IIngresoRepository repo)
{
    public async Task<IngresoListVM> GetListAsync(int mes, int anio)
    {
        var items = await repo.GetByMesAsync(mes, anio);

        return new IngresoListVM
        {
            Mes = mes, Anio = anio, Items = items,
            TotalPropio       = items.Where(i => i.Tipo == "Propio").Sum(i => i.Monto),
            TotalDistribuido  = items.Where(i => i.Tipo == "Distribuido").Sum(i => i.Monto),
            TotalCuentaPropia = items.Where(i => i.Tipo == "CuentaPropia").Sum(i => i.Monto),
            TotalAhorro       = items.Where(i => i.Tipo == "Ahorro").Sum(i => i.Monto),
            TotalDpto         = items.Where(i => i.Tipo == "Dpto").Sum(i => i.Monto),
            TotalUSS          = items.Where(i => i.Tipo == "USS").Sum(i => i.Monto),
            TotalFIMA         = items.Where(i => i.Tipo == "FIMA").Sum(i => i.Monto),
            TotalResto        = items.Where(i => i.Tipo == "Resto").Sum(i => i.Monto),
        };
    }

    public async Task SaveAsync(IngresoFormVM vm)
    {
        if (vm.Id == 0)
        {
            await repo.AddAsync(new Ingreso
            {
                Mes = vm.Mes, Anio = vm.Anio, Dia = vm.Dia,
                Tipo = vm.Tipo, Monto = vm.Monto,
                Descripcion = vm.Descripcion, Fuente = vm.Fuente
            });
        }
        else
        {
            var ingreso = await repo.GetByIdAsync(vm.Id)
                          ?? throw new KeyNotFoundException($"Ingreso {vm.Id} no encontrado");

            ingreso.Mes = vm.Mes; ingreso.Anio = vm.Anio; ingreso.Dia = vm.Dia;
            ingreso.Tipo = vm.Tipo; ingreso.Monto = vm.Monto;
            ingreso.Descripcion = vm.Descripcion; ingreso.Fuente = vm.Fuente;

            await repo.UpdateAsync(ingreso);
        }
    }

    public Task DeleteAsync(int id) => repo.DeleteAsync(id);
}
