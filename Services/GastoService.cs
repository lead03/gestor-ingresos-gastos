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

        decimal MontoEfectivo(GastoItem g) => g.MiParteMes;

        return new GastoListVM
        {
            Mes            = mes,
            Anio           = anio,
            Items          = items,
            TotalFijos        = items.Where(g => g.Categoria.Tipo == "Fijo"     && g.Moneda != "USD").Sum(MontoEfectivo),
            TotalVariables    = items.Where(g => g.Categoria.Tipo == "Variable" && g.Moneda != "USD").Sum(MontoEfectivo),
            TotalFijosUsd     = items.Where(g => g.Categoria.Tipo == "Fijo"     && g.Moneda == "USD").Sum(MontoEfectivo),
            TotalVariablesUsd = items.Where(g => g.Categoria.Tipo == "Variable" && g.Moneda == "USD").Sum(MontoEfectivo),
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
                vm.Moneda = g.Moneda;
                if (g.TarjetaCuota != null) vm.CantidadCuotas = g.TarjetaCuota.TotalCuotas;
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

        // Al editar con tarjeta: borrar cuotas viejas antes de crear las nuevas
        if (vm.Id != 0 && vm.TarjetaId.HasValue && vm.CantidadCuotas >= 1)
        {
            var gastoActual = await gastoRepo.GetByIdAsync(vm.Id);
            if (gastoActual?.TarjetaCuotaId.HasValue == true)
            {
                var primeraCuota = await tarjetaRepo.GetCuotaByIdAsync(gastoActual.TarjetaCuotaId.Value);
                if (primeraCuota != null)
                    await tarjetaRepo.DeleteCuotasGrupoAsync(
                        primeraCuota.TarjetaId,
                        primeraCuota.FechaCompra,
                        primeraCuota.MontoTotal,
                        primeraCuota.TotalCuotas);
            }
        }

        // Si paga con tarjeta y tiene más de 1 cuota, crear TarjetaCuotas automáticamente
        int? tarjetaCuotaId = vm.TarjetaCuotaId;
        if (vm.TarjetaId.HasValue && vm.CantidadCuotas >= 1)
        {
            var cuotaResult = await CrearCuotasAutomaticasAsync(vm);
            if (!cuotaResult.Success) return cuotaResult;
            tarjetaCuotaId = cuotaResult.Value; // ID de la primera cuota
        }

        int gastoId;
        if (vm.Id == 0)
        {
            var gasto = new GastoItem
            {
                Mes = vm.Mes, Anio = vm.Anio, Dia = vm.Dia,
                CategoriaId = vm.CategoriaId, Monto = vm.Monto,
                SeDivide = vm.SeDivide, MiParte = miParte,
                Descripcion = vm.Descripcion,
                CuentaId = vm.CuentaId, TarjetaId = vm.TarjetaId,
                TarjetaCuotaId = tarjetaCuotaId,
                Moneda = vm.Moneda
            };
            await gastoRepo.AddAsync(gasto);
            gastoId = gasto.Id;

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
            g.TarjetaCuotaId = tarjetaCuotaId;
            g.Moneda = vm.Moneda;

            await gastoRepo.UpdateAsync(g);
            await participanteRepo.DeleteByGastoAsync(vm.Id);
            gastoId = vm.Id;

            if (vm.SeDivide && vm.Participantes.Any())
                await GuardarParticipantesAsync(vm.Id, vm.Participantes);
        }

        // Actualizar GastoItemId en todas las cuotas del grupo
        if (vm.TarjetaId.HasValue && vm.CantidadCuotas >= 1)
        {
            var fechaCompra = new DateTime(vm.Anio, vm.Mes, vm.Dia);
            await tarjetaRepo.ActualizarGastoIdCuotasAsync(
                vm.TarjetaId.Value, fechaCompra, vm.Monto, vm.CantidadCuotas, gastoId);
        }

        return Result.Ok();
    }

    public Task DeleteAsync(int id) => gastoRepo.DeleteAsync(id);

    // ── Creación automática de cuotas ─────────────────────────────────
    private async Task<Result<int>> CrearCuotasAutomaticasAsync(GastoFormVM vm)
    {
        if (!vm.TarjetaId.HasValue)
            return Result.Fail<int>("No hay tarjeta seleccionada.");

        var tarjeta = await tarjetaRepo.GetByIdAsync(vm.TarjetaId.Value);
        if (tarjeta == null)
            return Result.Fail<int>("Tarjeta no encontrada.");

        var fechaCompra = new DateTime(vm.Anio, vm.Mes, vm.Dia);
        decimal montoCuota = Math.Round(vm.Monto / vm.CantidadCuotas, 2);

        // Usar DiaCierre del mes específico si existe; si no, el default de la tarjeta
        var fechaMensual = await tarjetaRepo.GetFechaMensualAsync(vm.TarjetaId.Value, vm.Mes, vm.Anio);
        int diaCierre    = fechaMensual?.DiaCierre ?? tarjeta.DiaCierre;

        // Determinar mes de cierre de la primera cuota
        // Si el día del gasto es ANTES o EN el día de cierre → cierra este mes
        // Si es DESPUÉS del día de cierre → cierra el mes siguiente
        int mesCierre  = fechaCompra.Day <= diaCierre
            ? fechaCompra.Month
            : (fechaCompra.Month == 12 ? 1 : fechaCompra.Month + 1);
        int anioCierre = fechaCompra.Day <= diaCierre
            ? fechaCompra.Year
            : (fechaCompra.Month == 12 ? fechaCompra.Year + 1 : fechaCompra.Year);

        int primeraCuotaId = 0;

        for (int i = 0; i < vm.CantidadCuotas; i++)
        {
            // Calcular mes/año de cada cuota
            int mesActual  = mesCierre  + i;
            int anioActual = anioCierre;
            while (mesActual > 12) { mesActual -= 12; anioActual++; }

            // Ajustar monto de la última cuota para absorber diferencia de redondeo
            decimal montoEstaCuota = i == vm.CantidadCuotas - 1
                ? vm.Monto - (montoCuota * (vm.CantidadCuotas - 1))
                : montoCuota;

            var cuota = new TarjetaCuota
            {
                TarjetaId     = vm.TarjetaId.Value,
                Comercio      = vm.Descripcion ?? "Sin descripción",
                FechaCompra   = fechaCompra,
                MontoTotal    = vm.Monto,
                TotalCuotas   = vm.CantidadCuotas,
                MontoCuota    = Math.Round(montoEstaCuota, 2),
                MesCierre     = mesActual,
                AnioCierre    = anioActual,
                CuotasPagadas = i,
                PagaParte     = "NO",
                Moneda        = vm.Moneda
            };

            await tarjetaRepo.AddCuotaAsync(cuota);

            if (i == 0) primeraCuotaId = cuota.Id;
        }

        return Result.Ok(primeraCuotaId);
    }

    // ── Participantes ─────────────────────────────────────────────────
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

    // ── Gestión de categorías ─────────────────────────────────────────
    public Task<List<CategoriaGasto>> GetTodasCategoriasAsync() =>
        gastoRepo.GetTodasCategoriasAsync();

    public async Task AgregarCategoriaAsync(string nombre, string tipo)
    {
        await gastoRepo.AddCategoriaAsync(new CategoriaGasto
        {
            Nombre     = nombre.Trim(),
            Tipo       = tipo,
            Habilitada = true
        });
    }

    public async Task EditarCategoriaAsync(int id, string nombre, string tipo)
    {
        var todas  = await gastoRepo.GetTodasCategoriasAsync();
        var cat    = todas.FirstOrDefault(c => c.Id == id);
        if (cat == null) return;
        cat.Nombre = nombre.Trim();
        cat.Tipo   = tipo;
        await gastoRepo.UpdateCategoriaAsync(cat);
    }

    /// <summary>
    /// Si tiene gastos → deshabilita (soft delete). Si no → elimina permanentemente.
    /// </summary>
    public async Task<string> DeshabilitarOEliminarAsync(int id)
    {
        if (await gastoRepo.CategoriaHasGastosAsync(id))
        {
            await gastoRepo.SetHabilitadaAsync(id, false);
            return "deshabilitada";
        }
        await gastoRepo.DeleteCategoriaAsync(id);
        return "eliminada";
    }

    public Task HabilitarCategoriaAsync(int id) =>
        gastoRepo.SetHabilitadaAsync(id, true);

    private async Task<List<CuentaResumenVM>> ObtenerCuentasConSaldoAsync()
    {
        var cuentas  = await cuentaRepo.GetAllActivasAsync();
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
