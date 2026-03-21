using ControlGastos.Common;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class GastoService(
    IGastoRepository             gastoRepo,
    IGastoParticipanteRepository participanteRepo,
    ITarjetaRepository           tarjetaRepo,
    IPersonaRepository           personaRepo,
    ICuentaRepository            cuentaRepo)
{
    public async Task<GastoListVM> GetListAsync(int mes, int anio)
    {
        var items = await gastoRepo.GetByMesAsync(mes, anio);

        decimal MontoEfectivo(GastoItem g) =>
            g.SeDivide ? g.MiParte ?? g.Monto : g.Monto;

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
        var cuentasConSaldo = await ObtenerCuentasConSaldoAsync();

        var vm = new GastoFormVM
        {
            Categorias = await gastoRepo.GetCategoriasAsync(),
            Cuotas     = await tarjetaRepo.GetCuotasActivasAsync(),
            Personas   = await personaRepo.GetAllAsync(),
            Cuentas    = cuentasConSaldo,
            Tarjetas   = await tarjetaRepo.GetAllAsync()
        };

        if (id.HasValue)
        {
            var g = await gastoRepo.GetByIdWithParticipantesAsync(id.Value);
            if (g != null)
            {
                vm.Id = g.Id; vm.Mes = g.Mes; vm.Anio = g.Anio; vm.Dia = g.Dia;
                vm.CategoriaId = g.CategoriaId; vm.Monto = g.Monto;
                vm.SeDivide = g.SeDivide; vm.Descripcion = g.Descripcion;
                vm.CuentaId = g.CuentaId; vm.TarjetaId = g.TarjetaId;
                vm.TarjetaCuotaId = g.TarjetaCuotaId;
                vm.Participantes = g.Participantes.Select(p => new ParticipanteFormVM
                {
                    Id          = p.Id,
                    Tipo        = p.Tipo.ToString(),
                    Descripcion = p.Descripcion,
                    Monto       = p.Monto,
                    PersonaId   = p.PersonaId
                }).ToList();
            }
        }
        return vm;
    }

    public async Task<Result> SaveAsync(GastoFormVM vm)
    {
        decimal miParte = vm.SeDivide
            ? vm.Participantes.Where(p => p.Tipo == "Yo").Sum(p => p.Monto)
            : vm.Monto;

        if (vm.Id == 0)
        {
            var gasto = new GastoItem
            {
                Mes = vm.Mes, Anio = vm.Anio, Dia = vm.Dia,
                CategoriaId = vm.CategoriaId, Monto = vm.Monto,
                SeDivide = vm.SeDivide, MiParte = miParte,
                Descripcion = vm.Descripcion,
                CuentaId = vm.CuentaId, TarjetaId = vm.TarjetaId,
                TarjetaCuotaId = vm.TarjetaCuotaId
            };
            await gastoRepo.AddAsync(gasto);

            if (vm.SeDivide && vm.Participantes.Any())
                await GuardarParticipantesAsync(gasto.Id, vm.Participantes);
        }
        else
        {
            var g = await gastoRepo.GetByIdAsync(vm.Id);
            if (g == null) return Result.Fail($"Gasto {vm.Id} no encontrado.");

            g.Mes = vm.Mes; g.Anio = vm.Anio; g.Dia = vm.Dia;
            g.CategoriaId = vm.CategoriaId; g.Monto = vm.Monto;
            g.SeDivide = vm.SeDivide; g.MiParte = miParte;
            g.Descripcion = vm.Descripcion;
            g.CuentaId = vm.CuentaId; g.TarjetaId = vm.TarjetaId;
            g.TarjetaCuotaId = vm.TarjetaCuotaId;

            await gastoRepo.UpdateAsync(g);
            await participanteRepo.DeleteByGastoAsync(vm.Id);

            if (vm.SeDivide && vm.Participantes.Any())
                await GuardarParticipantesAsync(vm.Id, vm.Participantes);
        }

        return Result.Ok();
    }

    public Task DeleteAsync(int id) => gastoRepo.DeleteAsync(id);

    private Task GuardarParticipantesAsync(int gastoId, List<ParticipanteFormVM> participantes)
    {
        var tipoMap = new Dictionary<string, TipoParticipante>
        {
            ["Yo"]      = TipoParticipante.Yo,
            ["Persona"] = TipoParticipante.Persona,
            ["Pagado"]  = TipoParticipante.Pagado
        };

        return participanteRepo.AddRangeAsync(participantes.Select(p => new GastoParticipante
        {
            GastoItemId = gastoId,
            Tipo        = tipoMap.GetValueOrDefault(p.Tipo, TipoParticipante.Yo),
            Descripcion = p.Descripcion,
            Monto       = p.Monto,
            PersonaId   = p.Tipo == "Persona" ? p.PersonaId : null
        }));
    }

    private async Task<List<CuentaResumenVM>> ObtenerCuentasConSaldoAsync()
    {
        var cuentas = await cuentaRepo.GetAllActivasAsync();
        var resultado = new List<CuentaResumenVM>();

        foreach (var c in cuentas)
        {
            var gastos   = await cuentaRepo.GetGastosByCuentaAsync(c.Id);
            var ingresos = await cuentaRepo.GetIngresosByCuentaAsync(c.Id);
            var saldo    = c.SaldoInicial
                         + ingresos.Sum(i => i.Monto)
                         - gastos.Sum(g => g.SeDivide ? g.MiParte ?? g.Monto : g.Monto);

            resultado.Add(new CuentaResumenVM
            {
                Id          = c.Id,
                Nombre      = c.Nombre,
                Tipo        = c.Tipo,
                SaldoActual = saldo,
                AlertaSaldo = c.AlertaSaldo
            });
        }
        return resultado;
    }
}
