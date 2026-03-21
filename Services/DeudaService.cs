using ControlGastos.Common;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class DeudaService(IDeudaRepository repo)
{
    public async Task<DeudaListVM> GetListAsync()
    {
        var deudas  = await repo.GetAllAsync();
        var meDeben = deudas.Where(d => d.Direccion == DireccionDeuda.MeDeben).ToList();
        var leDebo  = deudas.Where(d => d.Direccion == DireccionDeuda.LeDebo).ToList();

        return new DeudaListVM
        {
            MeDeben      = meDeben,
            LeDebo       = leDebo,
            TotalMeDeben = meDeben.Where(d => d.Estado != EstadoDeuda.Pagada)
                                  .Sum(d => d.Monto - (d.MontoPagado ?? 0)),
            TotalLeDebo  = leDebo.Where(d => d.Estado != EstadoDeuda.Pagada)
                                 .Sum(d => d.Monto - (d.MontoPagado ?? 0)),
        };
    }

    public async Task<Result> SaveAsync(DeudaFormVM vm)
    {
        if (vm.Id == 0)
        {
            await repo.AddAsync(new Deuda
            {
                PersonaId     = vm.PersonaId,
                NombrePersona = vm.NombrePersona,
                Monto         = vm.Monto,
                Fecha         = vm.Fecha,
                Descripcion   = vm.Descripcion,
                Direccion     = vm.Direccion,
                Estado        = vm.Estado,
                MontoPagado   = vm.MontoPagado
            });
        }
        else
        {
            var d = await repo.GetByIdAsync(vm.Id);
            if (d == null) return Result.Fail($"Deuda {vm.Id} no encontrada.");

            d.PersonaId     = vm.PersonaId;
            d.NombrePersona = vm.NombrePersona;
            d.Monto         = vm.Monto;
            d.Fecha         = vm.Fecha;
            d.Descripcion   = vm.Descripcion;
            d.Direccion     = vm.Direccion;
            d.Estado        = vm.Estado;
            d.MontoPagado   = vm.MontoPagado;

            await repo.UpdateAsync(d);
        }
        return Result.Ok();
    }

    public Task DeleteAsync(int id) => repo.DeleteAsync(id);
}
