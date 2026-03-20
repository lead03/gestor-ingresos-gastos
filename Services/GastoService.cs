using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class GastoService(IGastoRepository gastoRepo, ITarjetaRepository tarjetaRepo)
{
    public async Task<GastoListVM> GetListAsync(int mes, int anio)
    {
        var items = await gastoRepo.GetByMesAsync(mes, anio);

        decimal MontoEfectivo(GastoItem g) =>
            g.SeDivide ? g.MontoDividido ?? g.Monto : g.Monto;

        return new GastoListVM
        {
            Mes            = mes,
            Anio           = anio,
            Items          = items,
            TotalFijos     = items.Where(g => g.Categoria.Tipo == "Fijo").Sum(MontoEfectivo),
            TotalVariables = items.Where(g => g.Categoria.Tipo == "Variable").Sum(MontoEfectivo),
            PorDia         = items.GroupBy(g => g.Dia)
                                  .ToDictionary(grp => grp.Key, grp => grp.ToList())
        };
    }

    public async Task<GastoFormVM> GetFormAsync(int? id = null)
    {
        var vm = new GastoFormVM
        {
            Categorias = await gastoRepo.GetCategoriasAsync(),
            Cuotas     = await tarjetaRepo.GetCuotasActivasAsync()
        };

        if (id.HasValue)
        {
            var g = await gastoRepo.GetByIdAsync(id.Value);
            if (g != null)
            {
                vm.Id = g.Id; vm.Mes = g.Mes; vm.Anio = g.Anio; vm.Dia = g.Dia;
                vm.CategoriaId = g.CategoriaId; vm.Monto = g.Monto;
                vm.SeDivide = g.SeDivide; vm.CantidadPersonas = g.CantidadPersonas;
                vm.MontoDividido = g.MontoDividido; vm.Descripcion = g.Descripcion;
                vm.MedioPago = g.MedioPago; vm.TarjetaCuotaId = g.TarjetaCuotaId;
            }
        }
        return vm;
    }

    public async Task SaveAsync(GastoFormVM vm)
    {
        if (vm.SeDivide && vm.CantidadPersonas.HasValue && vm.CantidadPersonas > 0)
            vm.MontoDividido = Math.Round(vm.Monto / vm.CantidadPersonas.Value, 2);

        if (vm.Id == 0)
        {
            await gastoRepo.AddAsync(new GastoItem
            {
                Mes = vm.Mes, Anio = vm.Anio, Dia = vm.Dia,
                CategoriaId = vm.CategoriaId, Monto = vm.Monto,
                SeDivide = vm.SeDivide, CantidadPersonas = vm.CantidadPersonas,
                MontoDividido = vm.MontoDividido, Descripcion = vm.Descripcion,
                MedioPago = vm.MedioPago, TarjetaCuotaId = vm.TarjetaCuotaId
            });
        }
        else
        {
            var g = await gastoRepo.GetByIdAsync(vm.Id)
                    ?? throw new KeyNotFoundException($"Gasto {vm.Id} no encontrado");

            g.Mes = vm.Mes; g.Anio = vm.Anio; g.Dia = vm.Dia;
            g.CategoriaId = vm.CategoriaId; g.Monto = vm.Monto;
            g.SeDivide = vm.SeDivide; g.CantidadPersonas = vm.CantidadPersonas;
            g.MontoDividido = vm.MontoDividido; g.Descripcion = vm.Descripcion;
            g.MedioPago = vm.MedioPago; g.TarjetaCuotaId = vm.TarjetaCuotaId;

            await gastoRepo.UpdateAsync(g);
        }
    }

    public Task DeleteAsync(int id) => gastoRepo.DeleteAsync(id);
}
