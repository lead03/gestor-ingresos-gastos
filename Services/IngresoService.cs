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

        var totales = items
            .GroupBy(i => i.TipoIngreso?.Nombre ?? "—")
            .Select(g => (g.Key, g.Sum(i => i.Monto)))
            .OrderByDescending(x => x.Item2)
            .ToList();

        return new IngresoListVM { Mes = mes, Anio = anio, Items = items, TotalesPorTipo = totales };
    }

    public async Task<IngresoFormVM> GetFormAsync(int? id = null)
    {
        var cuentas = await cuentaRepo.GetAllActivasAsync();
        var tipos   = await repo.GetTiposAsync();

        var vm = new IngresoFormVM
        {
            Tipos   = tipos,
            Cuentas = cuentas.Select(c => new CuentaResumenVM { Id = c.Id, Nombre = c.Nombre, TipoEntidad = c.TipoEntidad, TipoNombre = c.TipoEntidad.ToString(), Moneda = c.Moneda }).ToList()
        };

        if (id.HasValue)
        {
            var ingreso = await repo.GetByIdFullAsync(id.Value);
            if (ingreso != null)
            {
                vm.Id           = ingreso.Id;
                vm.Mes          = ingreso.Mes;
                vm.Anio         = ingreso.Anio;
                vm.Dia          = ingreso.Dia;
                vm.TipoIngresoId = ingreso.TipoIngresoId;
                vm.Moneda       = ingreso.Moneda;
                vm.Monto        = ingreso.Monto;
                vm.Descripcion  = ingreso.Descripcion;
                vm.Distribuciones = ingreso.Distribuciones
                    .Select(d => new DistribucionFormVM { CuentaId = d.CuentaId, Monto = d.Monto })
                    .ToList();
            }
        }

        return vm;
    }

    public async Task<Result> SaveAsync(IngresoFormVM vm)
    {
        if (vm.TipoIngresoId <= 0)
            return Result.Fail("Debe seleccionar un tipo de ingreso.");
        if (vm.Monto <= 0 || vm.Monto > 9_999_999_999.99m)
            return Result.Fail("El monto debe ser mayor a $0 y menor a $9.999.999.999,99.");
        if (vm.Distribuciones.Any(d => d.Monto <= 0))
            return Result.Fail("Cada cuenta debe tener un monto mayor a $0.");

        // Validar que distribuciones sumen al monto total
        if (!vm.Distribuciones.Any())
            return Result.Fail("Debe asignar al menos una cuenta donde acreditar el ingreso.");

        var sumaDistr = vm.Distribuciones.Sum(d => d.Monto);
        if (Math.Abs(sumaDistr - vm.Monto) > 0.01m)
            return Result.Fail($"La suma de las cuentas ({sumaDistr:N2}) no coincide con el monto ({vm.Monto:N2}).");

        // Validar que no haya cuentas repetidas
        var cuentaIds = vm.Distribuciones.Select(d => d.CuentaId).ToList();
        if (cuentaIds.Distinct().Count() != cuentaIds.Count)
            return Result.Fail("No puede asignar la misma cuenta dos veces.");

        int ingresoId;
        if (vm.Id == 0)
        {
            var ingreso = new Ingreso
            {
                Mes = vm.Mes, Anio = vm.Anio, Dia = vm.Dia,
                TipoIngresoId = vm.TipoIngresoId,
                Moneda        = vm.Moneda,
                Monto         = vm.Monto,
                Descripcion   = vm.Descripcion
            };
            await repo.AddAsync(ingreso);
            ingresoId = ingreso.Id;
        }
        else
        {
            var ingreso = await repo.GetByIdAsync(vm.Id);
            if (ingreso == null) return Result.Fail($"Ingreso {vm.Id} no encontrado.");
            ingreso.Mes = vm.Mes; ingreso.Anio = vm.Anio; ingreso.Dia = vm.Dia;
            ingreso.TipoIngresoId = vm.TipoIngresoId;
            ingreso.Moneda        = vm.Moneda;
            ingreso.Monto         = vm.Monto;
            ingreso.Descripcion   = vm.Descripcion;
            await repo.UpdateAsync(ingreso);
            ingresoId = ingreso.Id;
            await repo.DeleteDistribucionesByIngresoAsync(ingresoId);
        }

        await repo.AddDistribucionesAsync(ingresoId,
            vm.Distribuciones.Select(d => new IngresoDistribucion { CuentaId = d.CuentaId, Monto = d.Monto }));

        return Result.Ok();
    }

    public Task DeleteAsync(int id) => repo.DeleteAsync(id);

    // ── Gestión de tipos de ingreso ───────────────────────────────
    public Task<List<TipoIngreso>> GetTodosTiposAsync() => repo.GetTodosTiposAsync();

    public async Task AgregarTipoAsync(string nombre) =>
        await repo.AddTipoAsync(new TipoIngreso { Nombre = nombre.Trim(), Habilitada = true });

    public async Task EditarTipoAsync(int id, string nombre)
    {
        var todos = await repo.GetTodosTiposAsync();
        var tipo  = todos.FirstOrDefault(t => t.Id == id);
        if (tipo == null) return;
        tipo.Nombre = nombre.Trim();
        await repo.UpdateTipoAsync(tipo);
    }

    public async Task<string> DeshabilitarOEliminarTipoAsync(int id)
    {
        if (await repo.TipoHasIngresosAsync(id))
        {
            await repo.SetTipoHabilitadoAsync(id, false);
            return "deshabilitado";
        }
        await repo.DeleteTipoAsync(id);
        return "eliminado";
    }

    public Task HabilitarTipoAsync(int id) => repo.SetTipoHabilitadoAsync(id, true);
}
